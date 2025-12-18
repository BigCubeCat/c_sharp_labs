using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Interface;
using Interface.Channel;
using Interface.DTO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Philosophers.Core.HostedServices;
using Philosophers.Services.Channels.Items;
using Xunit;

namespace Philosophers.Core.Tests;

public class DeadlockAnalyzerTests
{
    private DeadlockAnalyzer CreateAnalyzer(
        ChannelReader<PhilosopherToAnalyzerChannelItem> reader,
        ChannelWriter<PhilosopherToAnalyzerChannelItem> writer,
        out Mock<IChannel<PhilosopherToAnalyzerChannelItem>> channelMock,
        out Channel<PhilosopherToAnalyzerChannelItem> channel)
    {
        var logger = new Mock<ILogger<DeadlockAnalyzer>>();

        channel = Channel.CreateUnbounded<PhilosopherToAnalyzerChannelItem>();

        // Главный мок канала
        channelMock = new Mock<IChannel<PhilosopherToAnalyzerChannelItem>>();

        channelMock.Setup(c => c.Reader).Returns(reader);
        channelMock.Setup(c => c.Writer).Returns(writer);

        var dbContextMock = new Mock<ISimulationDatabaseProcessor>();
        dbContextMock
            .Setup(d =>
                d.SaveRunningInfoAsync(It.IsAny<RunningInfoDto>(), It.IsAny<CancellationToken>())
            )
            .Returns(Task.CompletedTask);

        var serviceMock = new Mock<IServiceProvider>();
        serviceMock
            .Setup(s => s.GetService(typeof(ISimulationDatabaseProcessor)))
            .Returns(dbContextMock.Object);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(serviceMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(s => s.CreateScope()).Returns(scopeMock.Object);

        // Notify вызывает у подписчиков SendMeItem,
        // но мы не хотим настоящей логики философа — просто заглушка
        channelMock.Setup(c => c.Notify(It.IsAny<object>()))
            .Callback<object>(o => { /* no-op */ });

        return new DeadlockAnalyzer(channelMock.Object, scopeFactoryMock.Object, logger.Object);
    }

    private void CallPublisherRegistered(DeadlockAnalyzer analyzer, Mock<IChannel<PhilosopherToAnalyzerChannelItem>> channelMock)
    {
        // эмулируем регистрацию N философов
        channelMock.Raise(c => c.PublisherWantToRegister += null, 0, EventArgs.Empty);
    }

    // ===================================================================
    //                         TEST 1: NO DEADLOCK
    // ===================================================================

    [Fact]
    public async Task NoDeadlock_When_Philosopher_IsEating()
    {
        // Arrange
        var channel = Channel.CreateUnbounded<PhilosopherToAnalyzerChannelItem>();

        var analyzer = CreateAnalyzer(channel.Reader, channel.Writer, out var channelMock, out var realChannel);

        // Регистрируем 2 философов
        CallPublisherRegistered(analyzer, channelMock);
        CallPublisherRegistered(analyzer, channelMock);

        var token = new CancellationTokenSource(2000).Token;

        // Первый философ — НЕ дедлок: он ест
        await channel.Writer.WriteAsync(new PhilosopherToAnalyzerChannelItem(
            IAmEating: true,
            LeftForkIsFree: false,
            RightForkIsFree: false));

        // Второй философ — не важно
        await channel.Writer.WriteAsync(new PhilosopherToAnalyzerChannelItem(
            IAmEating: false,
            LeftForkIsFree: false,
            RightForkIsFree: false));
        
        // Act
        var result = await Task.Run(async () =>
        {
            // private IsDeadlock(...) вызывается внутр. Analyze(),
            // но здесь мы тестируем именно Analyze()
            try
            {
                await analyzer.Analyze(token);
            }
            catch (ApplicationException ex)
            {
                return ex.Message == "Deadlock"; // если дедлок — плохой результат
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            return false; // Analyze() вернулся — значит дедлока нет
        });

        // Accept
        Assert.False(result); // дедлока быть не должно
    }

    // ===================================================================
    //                      TEST 2: DEADLOCK OCCURS
    // ===================================================================

    [Fact]
    public async Task Deadlock_Detected_When_AllForksBusy_And_NoOneIsEating()
    {
        // Arrange
        var channel = Channel.CreateUnbounded<PhilosopherToAnalyzerChannelItem>();

        var analyzer = CreateAnalyzer(channel.Reader, channel.Writer,
            out var channelMock, out var realChannel);

        // Регистрируем 3 философов
        CallPublisherRegistered(analyzer, channelMock);
        CallPublisherRegistered(analyzer, channelMock);
        CallPublisherRegistered(analyzer, channelMock);

        var cts = new CancellationTokenSource(2000);

        // Act
        // ВСЕ философы:
        // - не едят (IAmEating == false)
        // - обе вилки заняты (LeftForkIsFree=false, RightForkIsFree=false)
        // Это чистое условие дедлока.
        for (int i = 0; i < 3; i++)
        {
            await channel.Writer.WriteAsync(new PhilosopherToAnalyzerChannelItem(
                IAmEating: false,
                LeftForkIsFree: false,
                RightForkIsFree: false));
        }

        Assert.NotNull(await analyzer.Analyze(cts.Token));
    }

    // ===================================================================
    //           TEST 3: Not all forks are used → no deadlock
    // ===================================================================

    [Fact]
    public async Task NoDeadlock_When_AtLeastOneForkIsFree()
    {
        // Arrange
        var channel = Channel.CreateUnbounded<PhilosopherToAnalyzerChannelItem>();

        var analyzer = CreateAnalyzer(channel.Reader, channel.Writer,
            out var channelMock, out var realChannel);

        CallPublisherRegistered(analyzer, channelMock);
        CallPublisherRegistered(analyzer, channelMock);

        var token = new CancellationTokenSource(2000).Token;

        // 1-й философ — оба заняты
        await channel.Writer.WriteAsync(new PhilosopherToAnalyzerChannelItem(
            IAmEating: false,
            LeftForkIsFree: false,
            RightForkIsFree: false));

        // 2-й философ — хотя бы одна свободна
        await channel.Writer.WriteAsync(new PhilosopherToAnalyzerChannelItem(
            IAmEating: false,
            LeftForkIsFree: true,
            RightForkIsFree: false));

        // Act
        var result = await Task.Run(async () =>
        {
            try
            {
                await analyzer.Analyze(token);
            }
            catch (ApplicationException ex)
            {
                return ex.Message == "Deadlock"; // не должно произойти
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            return false; // нормальный выход
        });

        // Accept
        Assert.False(result);
    }

    // ===================================================================
    //     TEST 4: Multiple passes until deadlock appears dynamically
    // ===================================================================

    [Fact]
    public async Task Deadlock_Appears_On_Second_Pass()
    {  
        // Arrange
        var channel = Channel.CreateUnbounded<PhilosopherToAnalyzerChannelItem>();

        var analyzer = CreateAnalyzer(channel.Reader, channel.Writer,
            out var channelMock, out var realChannel);

        CallPublisherRegistered(analyzer, channelMock);
        CallPublisherRegistered(analyzer, channelMock);

        var token = new CancellationTokenSource(2000).Token;

        // Act
        // --- Первая итерация — нет дедлока ---
        // Философ 1 ест → дедлока нет
        await channel.Writer.WriteAsync(new PhilosopherToAnalyzerChannelItem(
            IAmEating: true,
            LeftForkIsFree: false,
            RightForkIsFree: false));

        // Философ 2 — заняты вилки, но это не важно
        await channel.Writer.WriteAsync(new PhilosopherToAnalyzerChannelItem(
            IAmEating: false,
            LeftForkIsFree: false,
            RightForkIsFree: false));

        // Analyze вернёт после первого цикла (нет дедлока)
        try
        {
            await analyzer.Analyze(token);
        }
        catch (OperationCanceledException) {}

        // --- Вторая итерация — дедлок ---
        // Оба не едят + все вилки заняты
        await channel.Writer.WriteAsync(new PhilosopherToAnalyzerChannelItem(
            IAmEating: false,
            LeftForkIsFree: false,
            RightForkIsFree: false));

        await channel.Writer.WriteAsync(new PhilosopherToAnalyzerChannelItem(
            IAmEating: false,
            LeftForkIsFree: false,
            RightForkIsFree: false));
        
        token = new CancellationTokenSource(2000).Token;

        // Accept
        Assert.NotNull(await analyzer.Analyze(token));
    }
}


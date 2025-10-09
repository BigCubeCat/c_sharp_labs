using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interface;
using Interface.Controller;

namespace Src.Controller;

public class Waiter : IWaiter
{
    private readonly Dictionary<IPhilosopherController, ClientEntity> _clients = [];
    private readonly Queue<ClientEntity> _hungryClients = [];

    private class ClientEntity
    {
        internal IPhilosopherController Client { get; set; }
        internal IForkController LeftFork { get; set; }
        internal IForkController RightFork { get; set; }
        internal bool IAmLeftHanded { get; set; }

        internal ClientEntity(IPhilosopherController client, IForkController leftFork, IForkController rightFork, bool isLeftHanded)
        {
            Client = client;
            LeftFork = leftFork;
            RightFork = rightFork;

            IAmLeftHanded = isLeftHanded;
        }
    }

    public event EventHandler<IPhilosopherController> YouHasRightForkNotify;
    public event EventHandler<IPhilosopherController> YouHasLeftForkNotify;

    public static IWaiter Create(List<IPhilosopherController> clients, List<IForkController> forks, bool isDeadlockConfigure)
    {
        return new Waiter(clients, forks, isDeadlockConfigure);
    }

    public Waiter(List<IPhilosopherController> clients, List<IForkController> forks, bool isDeadlockConfigure)
    {
        for (int i = 0; i < forks.Count; ++i)
        {
            clients[i].IAmFullNotify += ClientIsFullHandler;
            clients[i].IAmHungryNotify += ClientIsHungryHandler;
            clients[i].INeedLeftForkNotify += ClientNeedsLeftForkHandler;
            clients[i].INeedRightForkNotify += ClientNeedsRightForkHandler;
            clients[i].ICanTakeLeftForkNotify += ClientCanTakeLeftForkHandler;
            clients[i].ICanTakeRightForkNotify += ClientCanTakeRightForkHandler;
            clients[i].AddWaiterAndSubscribeOnHisEvents(this);
            _clients.Add(clients[i], new ClientEntity(clients[i], forks[(i + 1) % forks.Count], forks[i], false));
        }

        if (!isDeadlockConfigure) _clients.Last().Value.IAmLeftHanded = true;
    }

    private void ClientIsHungryHandler(object? sender, EventArgs e)
    {
        if (sender is IPhilosopherController client)
        {
            if (!_hungryClients.Contains(_clients[client]))
                _hungryClients.Enqueue(_clients[client]);
        }
    }

    private void ClientIsFullHandler(object? sender, EventArgs e)
    {
        if (sender is IPhilosopherController client)
        {
            _clients[client].LeftFork.Put();
            _clients[client].RightFork.Put();
        }
    }

    private void ClientNeedsLeftForkHandler(object? sender, EventArgs e)
    {
        if (sender is IPhilosopherController client)
        {
            if (!_hungryClients.Contains(_clients[client]))
                _hungryClients.Enqueue(_clients[client]);
        }
    }

    private void ClientNeedsRightForkHandler(object? sender, EventArgs e)
    {
        if (sender is IPhilosopherController client)
        {
            if (!_hungryClients.Contains(_clients[client]))
                _hungryClients.Enqueue(_clients[client]);
        }
    }

    private void ClientCanTakeRightForkHandler(object? sender, EventArgs e)
    {
        if (sender is IPhilosopherController client && _clients[client].RightFork.IsLockedBy(_clients[client].Client))
        {
            _clients[client].RightFork.Take(_clients[client].Client);
        }
    }

    private void ClientCanTakeLeftForkHandler(object? sender, EventArgs e)
    {
        if (sender is IPhilosopherController client && _clients[client].LeftFork.IsLockedBy(_clients[client].Client))
        {
            _clients[client].LeftFork.Take(_clients[client].Client);
        }
    }

    public void Step()
    {
        ClientEntity client;
        int count = _hungryClients.Count;
        while (count != 0)
        {
            client = _hungryClients.Dequeue();

            if (!client.LeftFork.IsLocked() && !client.RightFork.IsLocked())
            {
                if (client.IAmLeftHanded)
                {
                    client.LeftFork.Lock(client.Client);
                    YouHasLeftForkNotify(this, client.Client);
                }
                else
                {
                    client.RightFork.Lock(client.Client);
                    YouHasRightForkNotify(this, client.Client);
                }
            }
            else if (!client.LeftFork.IsLocked() && client.RightFork.IsLockedBy(client.Client))
            {
                client.LeftFork.Lock(client.Client);
                YouHasLeftForkNotify(this, client.Client);
            }
            else if (!client.RightFork.IsLocked() && client.LeftFork.IsLockedBy(client.Client))
            {
                client.RightFork.Lock(client.Client);
                YouHasRightForkNotify(this, client.Client);
            }
            else
            {
                _hungryClients.Enqueue(client);
            }

            --count;
        }
    }
}


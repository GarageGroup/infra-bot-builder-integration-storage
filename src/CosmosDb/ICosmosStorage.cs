using System;
using Microsoft.Bot.Builder;

namespace GarageGroup.Infra.Bot.Builder;

public interface ICosmosStorage : IStorage, IPingSupplier, IDisposable
{
}
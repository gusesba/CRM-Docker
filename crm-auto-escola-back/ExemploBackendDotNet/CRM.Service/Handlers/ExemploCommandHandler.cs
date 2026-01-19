using Exemplo.Domain.Model;
using Exemplo.Persistence;
using Exemplo.Service.Commands;
using MediatR;

namespace Exemplo.Service.Handlers
{
    public class ExemploCommandHandler : IRequestHandler<ExemploCommand,ExemploModel>
    {
        private readonly ExemploDbContext _exemploDbContext;

        public ExemploCommandHandler(ExemploDbContext exemploDbContext) 
        { 
            _exemploDbContext = exemploDbContext;
        }

        public async Task<ExemploModel> Handle(ExemploCommand command, CancellationToken cancellationToken)
        {
            ExemploModel exemploModel = new() 
            { 
                Campo2 = command.Campo2,
                Campo3 = command.Campo3 
            };
            var result = await _exemploDbContext.Exemplo.AddAsync(exemploModel,cancellationToken);
            await _exemploDbContext.SaveChangesAsync(cancellationToken);

            return result.Entity;
        }
    }
}

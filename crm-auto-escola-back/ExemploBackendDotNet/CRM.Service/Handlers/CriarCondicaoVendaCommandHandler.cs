using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Dto;
using Exemplo.Persistence;
using Exemplo.Service.Commands;
using Exemplo.Service.Exceptions;
using Exemplo.Service.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class CriarCondicaoVendaCommandHandler : IRequestHandler<CriarCondicaoVendaCommand, CondicaoVendaModel>
    {
        private readonly ExemploDbContext _context;
        public CriarCondicaoVendaCommandHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<CondicaoVendaModel> Handle(CriarCondicaoVendaCommand request, CancellationToken cancellationToken)
        {
            var condicaoVendaExistente = await _context.CondicaoVenda
                .FirstOrDefaultAsync(u => u.Nome == request.Nome);

            if (condicaoVendaExistente != null)
                throw new ConflictException("Condição de venda já cadastrada.");

            var novoCondicaoVenda = new CondicaoVendaModel()
            {
                Nome = request.Nome,
            };

            var condicaoVenda = await _context.CondicaoVenda.AddAsync(novoCondicaoVenda);
            await _context.SaveChangesAsync();

            return condicaoVenda.Entity;
        }
    }
}

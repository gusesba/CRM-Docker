using Exemplo.Persistence;
using Exemplo.Service.Commands;
using Exemplo.Service.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class TransferirVendasCommandHandler
        : IRequestHandler<TransferirVendasCommand, Unit>
    {
        private readonly ExemploDbContext _context;

        public TransferirVendasCommandHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(
            TransferirVendasCommand request,
            CancellationToken cancellationToken)
        {
            if (request.VendasIds == null || !request.VendasIds.Any())
                throw new ValidationException("Nenhuma venda foi selecionada para transferência.");

            // Buscar todas as vendas informadas
            var vendas = await _context.Venda
                .Where(v => request.VendasIds.Contains(v.Id))
                .ToListAsync(cancellationToken);

            if (!vendas.Any())
                throw new NotFoundException("Nenhuma venda encontrada para os IDs informados.");

            // Atualizar vendedor das vendas
            foreach (var venda in vendas)
            {
                //  Ajuste o nome da propriedade conforme seu Model
                venda.VendedorAtualId = request.UsuarioId;
                if (request.Permanente)
                {
                    venda.VendedorId = request.UsuarioId;
                }
                venda.DataAlteracao = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}

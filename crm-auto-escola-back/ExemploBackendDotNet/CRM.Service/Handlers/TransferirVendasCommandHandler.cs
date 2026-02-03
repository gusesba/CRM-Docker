using Exemplo.Persistence;
using Exemplo.Service.Commands;
using Exemplo.Service.Exceptions;
using Exemplo.Service.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class TransferirVendasCommandHandler
        : IRequestHandler<TransferirVendasCommand, Unit>
    {
        private readonly ExemploDbContext _context;
        private readonly IUsuarioContextService _usuarioContextService;

        public TransferirVendasCommandHandler(
            ExemploDbContext context,
            IUsuarioContextService usuarioContextService)
        {
            _context = context;
            _usuarioContextService = usuarioContextService;
        }

        public async Task<Unit> Handle(
            TransferirVendasCommand request,
            CancellationToken cancellationToken)
        {
            var access = await _usuarioContextService.GetUsuarioSedeAccessAsync(cancellationToken);
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
                access.EnsureSameSede(venda.SedeId, "Venda não pertence à sua sede.");

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

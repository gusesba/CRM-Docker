using Exemplo.Domain.Model;
using Exemplo.Persistence;
using Exemplo.Service.Commands;
using Exemplo.Service.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class CriarAgendamentoCommandHandler : IRequestHandler<CriarAgendamentoCommand, AgendamentoModel>
    {
        private readonly ExemploDbContext _context;

        public CriarAgendamentoCommandHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<AgendamentoModel> Handle(
            CriarAgendamentoCommand request,
            CancellationToken cancellationToken)
        {
            // Verifica se a venda existe
            var venda = await _context.Venda
                .FirstOrDefaultAsync(v => v.Id == request.VendaId, cancellationToken);

            if (venda == null)
                throw new NotFoundException("Venda não encontrada.");

            // Cria o novo agendamento
            var novoAgendamento = new AgendamentoModel
            {
                VendaId = request.VendaId,
                DataAgendamento = DateTime.SpecifyKind(request.DataAgendamento, DateTimeKind.Utc),
                Obs = request.Obs
            };

            var agendamento = await _context.Agendamento.AddAsync(novoAgendamento, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return agendamento.Entity;
        }
    }
}

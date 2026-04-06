using Exemplo.Domain.Model;
using Exemplo.Domain.Model.Dto;
using Exemplo.Persistence;
using Exemplo.Service.Commands;
using Exemplo.Service.Exceptions;
using Exemplo.Service.Helpers;
using Exemplo.Service.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class CriarVendaCommandHandler : IRequestHandler<CriarVendaCommand, VendaModel>
    {
        private readonly ExemploDbContext _context;
        public CriarVendaCommandHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<VendaModel> Handle(CriarVendaCommand request, CancellationToken cancellationToken)
        {
            var contatosParaComparar = ContatoNormalization.BuildPhoneVariants(request.Contato).ToList();
            var vendasEquivalentes = await _context.Venda
                .AsNoTracking()
                .Where(v => v.Contato != null)
                .Select(v => new { v.Id, v.Contato, v.SedeId })
                .Where(v => contatosParaComparar.Contains(v.Contato!))
                .OrderBy(v => v.Id)
                .ToListAsync(cancellationToken);

            var vendaExistenteNaMesmaSede = vendasEquivalentes
                .FirstOrDefault(v => v.SedeId == request.SedeId);

            if (vendaExistenteNaMesmaSede != null)
                throw new ConflictException("Venda já cadastrada.");

            if (!string.IsNullOrWhiteSpace(request.ObsRetorno) && request.DataRetorno == null)
                throw new ConflictException("Data de retorno é obrigatória quando a observação de retorno é informada.");

            var contatoCanonico = vendasEquivalentes.FirstOrDefault()?.Contato ?? request.Contato;

            var novoVenda = new VendaModel()
            {
                ComoConheceu = request.ComoConheceu,
                VendedorId = request.VendedorId,
                ValorVenda = request.ValorVenda,
                Status = request.Status,
                ServicoId = request.ServicoId,
                SedeId = request.SedeId,
                Origem = request.Origem,
                Obs = request.Obs,
                CondicaoVendaId = request.CondicaoVendaId,
                Contato = contatoCanonico,
                Email = request.Email,
                DataInicial = DateTime.UtcNow,
                Fone = request.Fone,
                Genero = request.Genero,
                Indicacao = request.Indicacao,
                MotivoEscolha = request.MotivoEscolha,
                Cliente = request.Cliente,
                DataNascimento = request.DataNascimento,
                VendedorAtualId = request.VendedorId
            };

            var venda = await _context.Venda.AddAsync(novoVenda, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            if (request.DataRetorno.HasValue)
            {
                var novoAgendamento = new AgendamentoModel
                {
                    VendaId = venda.Entity.Id,
                    DataAgendamento = DateTime.SpecifyKind(request.DataRetorno.Value, DateTimeKind.Utc),
                    Obs = request.ObsRetorno ?? string.Empty
                };

                await _context.Agendamento.AddAsync(novoAgendamento, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return venda.Entity;
        }
    }
}

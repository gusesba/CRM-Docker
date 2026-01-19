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
    public class CriarVendaCommandHandler : IRequestHandler<CriarVendaCommand, VendaModel>
    {
        private readonly ExemploDbContext _context;
        public CriarVendaCommandHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<VendaModel> Handle(CriarVendaCommand request, CancellationToken cancellationToken)
        {
            var vendaExistente = await _context.Venda
               .FirstOrDefaultAsync(u => u.Contato == request.Contato);

            if (vendaExistente != null)
                throw new ConflictException("Venda já cadastrada.");

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
                Contato = request.Contato,
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

            var venda = await _context.Venda.AddAsync(novoVenda);
            await _context.SaveChangesAsync();

            return venda.Entity;
        }
    }
}

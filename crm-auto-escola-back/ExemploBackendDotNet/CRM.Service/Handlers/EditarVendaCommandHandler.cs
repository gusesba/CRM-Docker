using Exemplo.Domain.Model;
using Exemplo.Persistence;
using Exemplo.Service.Commands;
using Exemplo.Service.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class EditarVendaCommandHandler : IRequestHandler<EditarVendaCommand, VendaModel>
    {
        private readonly ExemploDbContext _context;

        public EditarVendaCommandHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<VendaModel> Handle(EditarVendaCommand request, CancellationToken cancellationToken)
        {
            var venda = await _context.Venda
                .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

            if (venda == null)
                throw new NotFoundException("Venda não encontrada.");

            venda.ComoConheceu = request.ComoConheceu;
            venda.VendedorId = request.VendedorId;
            venda.ValorVenda = request.ValorVenda;
            venda.Status = request.Status;
            venda.ServicoId = request.ServicoId;
            venda.SedeId = request.SedeId;
            venda.Origem = request.Origem;
            venda.Obs = request.Obs;
            venda.CondicaoVendaId = request.CondicaoVendaId;
            venda.Contato = request.Contato;
            venda.Email = request.Email;
            venda.Fone = request.Fone;
            venda.Genero = request.Genero;
            venda.Indicacao = request.Indicacao;
            venda.MotivoEscolha = request.MotivoEscolha;
            venda.Cliente = request.Cliente;
            venda.DataNascimento = request.DataNascimento;

            venda.DataAlteracao = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return venda;
        }
    }
}

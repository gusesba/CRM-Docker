using Exemplo.Domain.Model;
using Exemplo.Persistence;
using Exemplo.Service.Commands;
using Exemplo.Service.Exceptions;
using Exemplo.Service.Helpers;
using Exemplo.Service.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class EditarVendaCommandHandler : IRequestHandler<EditarVendaCommand, VendaModel>
    {
        private readonly ExemploDbContext _context;
        private readonly IUsuarioContextService _usuarioContextService;

        public EditarVendaCommandHandler(
            ExemploDbContext context,
            IUsuarioContextService usuarioContextService)
        {
            _context = context;
            _usuarioContextService = usuarioContextService;
        }

        public async Task<VendaModel> Handle(EditarVendaCommand request, CancellationToken cancellationToken)
        {
            var access = await _usuarioContextService.GetUsuarioSedeAccessAsync(cancellationToken);
            var venda = await _context.Venda
                .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

            if (venda == null)
                throw new NotFoundException("Venda não encontrada.");

            access.EnsureSameSede(venda.SedeId, "Venda não pertence à sua sede.");

            if (request.SedeId.HasValue)
            {
                access.EnsureSameSede(request.SedeId, "Não é permitido alterar a sede da venda.");
                venda.SedeId = request.SedeId;
            }

            var contatosParaComparar = ContatoNormalization.BuildPhoneVariants(request.Contato).ToList();
            var vendaExistente = await _context.Venda
                .AsNoTracking()
                .Where(v => v.Id != venda.Id && v.Contato != null)
                .Select(v => new { v.Id, v.Contato })
                .FirstOrDefaultAsync(v => contatosParaComparar.Contains(v.Contato!), cancellationToken);

            if (vendaExistente != null)
                throw new ConflictException("Venda já cadastrada.");

            venda.ComoConheceu = request.ComoConheceu;
            venda.VendedorId = request.VendedorId;
            venda.ValorVenda = request.ValorVenda;
            venda.Status = request.Status;
            venda.ServicoId = request.ServicoId;
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

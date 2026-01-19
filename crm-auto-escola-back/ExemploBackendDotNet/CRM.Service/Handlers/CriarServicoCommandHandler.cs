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
    public class CriarServicoCommandHandler : IRequestHandler<CriarServicoCommand, ServicoModel>
    {
        private readonly ExemploDbContext _context;
        public CriarServicoCommandHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<ServicoModel> Handle(CriarServicoCommand request, CancellationToken cancellationToken)
        {
            var servicoExistente = await _context.Servico
                .FirstOrDefaultAsync(u => u.Nome == request.Nome);

            if (servicoExistente != null)
                throw new ConflictException("Serviço já cadastrado.");

            var novoServico = new ServicoModel()
            {
                Nome = request.Nome,
            };

            var servico = await _context.Servico.AddAsync(novoServico);
            await _context.SaveChangesAsync();

            return servico.Entity;
        }
    }
}

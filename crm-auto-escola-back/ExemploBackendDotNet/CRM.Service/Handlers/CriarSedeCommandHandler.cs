using Exemplo.Domain.Model;
using Exemplo.Persistence;
using Exemplo.Service.Commands;
using Exemplo.Service.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exemplo.Service.Handlers
{
    public class CriarSedeCommandHandler : IRequestHandler<CriarSedeCommand, SedeModel>
    {
        private readonly ExemploDbContext _context;
        public CriarSedeCommandHandler(ExemploDbContext context)
        {
            _context = context;
        }

        public async Task<SedeModel> Handle(CriarSedeCommand request, CancellationToken cancellationToken)
        {
            var sedeExistente = await _context.Sede
                .FirstOrDefaultAsync(u => u.Nome == request.Nome);

            if (sedeExistente != null)
                throw new ConflictException("Sede já cadastrada.");

            var novaSede = new SedeModel()
            {
                Nome = request.Nome,
                Ativo = request.Ativo,
                DataInclusao = request.DataInclusao,
            };

            var sede = await _context.Sede.AddAsync(novaSede);
            await _context.SaveChangesAsync();

            return sede.Entity;
        }
    }
}

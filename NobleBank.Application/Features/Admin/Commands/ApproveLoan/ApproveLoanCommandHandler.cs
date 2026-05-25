using MediatR;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Exceptions;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;
namespace NobleBank.Application.Features.Admin.Commands.ApproveLoan
{
    public class ApproveLoanCommandHandler : IRequestHandler<ApproveLoanCommand>
    {
        private readonly IApplicationDbContext _context;

        public ApproveLoanCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ApproveLoanCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.AdminUserId))
            {
                throw new UnauthorizedAccessException(Constants.Exceptions.UserNotFound);
            }

            Loan? loan = await _context.Loans
                .FirstOrDefaultAsync(l => l.Id == request.LoanId, cancellationToken);

            if (loan is null)
            {
                throw new NotFoundException(Constants.Exceptions.LoanNotFound);
            }

            if (loan.Status != LoansEnum.Status.Pending)
            {
                throw new DomainException($"Cannot approve loan in {loan.Status} status");
            }

            loan.Approve();

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

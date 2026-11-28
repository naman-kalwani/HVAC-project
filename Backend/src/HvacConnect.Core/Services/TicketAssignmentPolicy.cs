using HvacConnect.Core.Domain.Entities;
using HvacConnect.Core.Domain.Enums;

namespace HvacConnect.Core.Services;

public static class TicketAssignmentPolicy
{
    public static (bool IsAllowed, string ErrorMessage) AuthorizeTicket(User actor, ServiceTicket ticket)
    {
        if (actor.Role is not (UserRole.SuperAdmin or UserRole.VendorAdmin or UserRole.ServiceCoordinator))
        {
            return (false, "Your role is not permitted to assign technicians.");
        }

        if (ticket.Status is not (TicketStatus.Open or TicketStatus.Assigned))
        {
            return (false, "Technicians can only be assigned to Open or Assigned tickets.");
        }

        if (actor.Role == UserRole.VendorAdmin &&
            (actor.VendorId == null || ticket.VendorId != actor.VendorId))
        {
            return (false, "This ticket is outside your vendor scope.");
        }

        if (actor.Role == UserRole.ServiceCoordinator &&
            ticket.OwnerCoordinatorId != null &&
            ticket.OwnerCoordinatorId != actor.Id)
        {
            return (false, "Only the ticket's owning coordinator may assign a technician.");
        }

        return (true, string.Empty);
    }

    public static (bool IsAllowed, string ErrorMessage) AuthorizeTechnician(
        User actor,
        ServiceTicket ticket,
        User technician)
    {
        var ticketAuthorization = AuthorizeTicket(actor, ticket);
        if (!ticketAuthorization.IsAllowed)
        {
            return ticketAuthorization;
        }

        if (technician.Role != UserRole.Technician || !technician.IsActive)
        {
            return (false, "Select an active technician.");
        }

        if (technician.AvailabilityStatus == TechnicianAvailability.OffDuty)
        {
            return (false, "An OffDuty technician cannot be assigned.");
        }

        if (technician.VendorId == null ||
            technician.Vendor == null ||
            !technician.Vendor.IsActive ||
            technician.Vendor.Status != VendorStatus.Active)
        {
            return (false, "The technician must belong to an active vendor.");
        }

        if (actor.Role == UserRole.VendorAdmin && technician.VendorId != actor.VendorId)
        {
            return (false, "You can only assign technicians from your vendor.");
        }

        return (true, string.Empty);
    }
}

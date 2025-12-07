using LogisticsAndDeliveries.Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogisticsAndDeliveries.Domain.Drivers
{
    public static class DriverErrors
    {
        public static readonly Error DriverNotFound = new(
            "Driver.NotFound",
            "The requested driver was not found.",
            ErrorType.NotFound);

        public static Error NameIsRequired() => new(
            "Driver.NameIsRequired",
            "The driver's name is required.",
            ErrorType.Validation);

        public static Error InvalidLatitude() => new(
            "Driver.InvalidLatitude",
            "The latitude must be between -90 and 90 degrees.",
            ErrorType.Validation);

        public static Error InvalidLongitude() => new(
            "Driver.InvalidLongitude",
            "The longitude must be between -180 and 180 degrees.",
            ErrorType.Validation);
    }
}

using LogisticsAndDeliveries.Core.Results;

namespace LogisticsAndDeliveries.Domain.Packages
{
    public static class PackageErrors
    {
        public static readonly Error PackageNotFound = new(
            "Package.NotFound",
            "The requested package was not found.",
            ErrorType.NotFound);

        public static Error NumberIsRequired() => new(
            "Package.NumberIsRequired",
            "The package number is required.",
            ErrorType.Validation);

        public static Error PatientIdIsRequired() => new(
            "Package.PatientIdIsRequired",
            "The patient ID is required.",
            ErrorType.Validation);
        public static Error PatientNameIsRequired() => new(
            "Package.PatientNameIsRequired",
            "The patient's name is required.",
            ErrorType.Validation);

        public static Error PatientPhoneIsRequired() => new(
            "Package.PatientPhoneIsRequired",
            "The patient's phone number is required.",
            ErrorType.Validation);

        public static Error DeliveryAddressIsRequired() => new(
            "Package.DeliveryAddressIsRequired",
            "The delivery address is required.",
            ErrorType.Validation);

        public static Error InvalidDeliveryLatitude() => new(
            "Package.InvalidDeliveryLatitude",
            "The delivery latitude must be between -90 and 90 degrees.",
            ErrorType.Validation);

        public static Error InvalidDeliveryLongitude() => new(
            "Package.InvalidDeliveryLongitude",
            "The delivery longitude must be between -180 and 180 degrees.",
            ErrorType.Validation);

        public static Error InvalidDeliveryDate() => new(
            "Package.InvalidDeliveryDate",
            "The delivery date cannot be in the past.",
            ErrorType.Validation);

        public static Error DriverIdIsRequired() => new(
            "Package.DriverIdIsRequired",
            "The driver ID is required.",
            ErrorType.Validation);
    }
}

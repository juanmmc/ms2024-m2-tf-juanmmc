using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogisticsAndDeliveries.Infrastructure.Persistence.PersistenceModel.EFCoreEntities
{
    [Table("driver")]
    public class DriverPersistenceModel
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        [MaxLength(200)]
        [Required]
        public required string Name { get; set; }

        [Column("latitude", TypeName = "double precision")]
        public double? Latitude { get; set; }

        [Column("longitude", TypeName = "double precision")]
        public double? Longitude { get; set; }

        [Column("lastLocationUpdate")]
        public DateTime? LastLocationUpdate { get; set; }
    }
}

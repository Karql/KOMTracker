using System.Collections.Generic;

namespace KOMTracker.API.Models.Segment
{
    public class SegmentModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ActivityType { get; set; }
        public float Distance { get; set; }
        public float AverageGrade { get; set; }
        public float MaximumGrade { get; set; }
        public float ElevationHigh { get; set; }
        public float ElevationLow { get; set; }
        public float StartLatitude { get; set; }
        public float StartLongitude { get; set; }
        public float EndLatitude { get; set; }
        public float EndLongitude { get; set; }
        public int ClimbCategory { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public bool Private { get; set; }
        public bool Hazardous { get; set; }
        public bool Starred { get; set; }
        
        // TODO: Detailed fields
    }
}
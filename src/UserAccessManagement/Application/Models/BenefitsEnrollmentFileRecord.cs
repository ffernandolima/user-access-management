using FileHelpers;
using FileHelpers.Events;
using System;

namespace UserAccessManagement.Application.Models
{
    [IgnoreEmptyLines]
    [DelimitedRecord(";")]
    public class BenefitsEnrollmentFileRecord : INotifyRead
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Country { get; set; }

        [FieldConverter(ConverterKind.Date, "MM-dd-yyyy")]
        public DateTime? BirthDate { get; set; }

        [FieldConverter(ConverterKind.Decimal)]
        public decimal? Salary { get; set; }

        [FieldHidden]
        public int LineNumber { get; private set; }

        [FieldHidden]
        public string RecordString { get; private set; }

        public void BeforeRead(BeforeReadEventArgs e)
        {
            e.SkipThisRecord = false;
        }

        public void AfterRead(AfterReadEventArgs e)
        {
            LineNumber = e.LineNumber;
            RecordString = e.RecordLine;

            e.SkipThisRecord = false;
        }
    }
}

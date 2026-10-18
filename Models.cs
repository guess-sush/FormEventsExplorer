using System;

namespace FormEventExplorerXrmToolBox
{
    internal sealed class TableItem
    {
        public string LogicalName { get; set; }
        public string DisplayName { get; set; }
        public override string ToString() => $"{DisplayName} ({LogicalName})";
    }

    internal sealed class FormItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FormXml { get; set; }
        public override string ToString() => Name;
    }

    internal sealed class EventRow
    {
        public string Event { get; set; }
        public string Field { get; set; }
        public string Control { get; set; }
        public string Function { get; set; }
        public string Library { get; set; }
        public bool Enabled { get; set; }
        public bool PassContext { get; set; }
    }

    internal sealed class RuleRow
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Scope { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}

namespace Bacera.Gateway;

#region {{Accounting}}

partial class Charge : IEntity<int> { }
partial class Currency : IEntity<int> { }
partial class Invoice : IEntity { }
partial class Ledger : IEntity { }
partial class FundType : IEntity<int> { }

#endregion

#region {{Core}}
partial class Action : IEntity<int> { }
partial class Matter : IEntity { }
partial class MatterType : IEntity<int> { }
partial class Party : IEntity { }
partial class PartyRole : IEntity { }
partial class State : IEntity<int> { }
partial class Supplement : IEntity { }
partial class Transition : IEntity { }

#endregion

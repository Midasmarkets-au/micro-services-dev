export enum LeadStatusTypes {
  UserNotRegistered = 0,
  UserRegistered = 10,
  UserVerifying = 20,
  UserVerificationRejected = 26,
  AccountApplicationCreated = 30, // tenant approves verification, both agent and client
  AccountApplicationApproved = 31, // tenant approves client's account application
  AccountApplicationRejected = 35,
  TradeAccountCreated = 40, // tenant opens MT5 account for client
  AgentAccountCreated = 50,
}

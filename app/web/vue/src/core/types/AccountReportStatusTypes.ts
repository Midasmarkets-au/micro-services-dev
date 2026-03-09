export enum AccountReportStatusTypes {
  Initialed = 0,
  Generated = 10,
  MailSent = 20,
}

export const AccountReportStatusOptions = [
  { value: AccountReportStatusTypes.Initialed, label: "Initialed" },
  { value: AccountReportStatusTypes.Generated, label: "Generated" },
  { value: AccountReportStatusTypes.MailSent, label: "Mail Sent" },
];

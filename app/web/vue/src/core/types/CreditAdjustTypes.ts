import i18n from "@/core/plugins/i18n";
const { t } = i18n.global;

export enum BacthStatus {
  created = 0,
  pending = 10,
  processing = 20,
  completed = 30,
  failed = 40,
}

export enum AdjustRecordStatusTypes {
  created = 0,
  processing = 10,
  completed = 20,
  failed = 30,
}

export enum creditAdjustTypes {
  Adjust = 1,
  Credit = 2,
  Agent = 3,
}

export const creditAdjustTypeOptions = [
  { value: creditAdjustTypes.Credit, label: t("fields.credit") },
  { value: creditAdjustTypes.Adjust, label: t("fields.adjust") },
  { value: creditAdjustTypes.Agent, label: t("fields.agent") },
];

export const batchStatusOptions = [
  { value: BacthStatus.created, label: t("status.created") },
  { value: BacthStatus.processing, label: t("status.processing") },
  { value: BacthStatus.completed, label: t("status.completed") },
  { value: BacthStatus.failed, label: t("status.failed") },
];

export const adjustRecordStatusOptions = [
  { value: AdjustRecordStatusTypes.created, label: t("status.created") },
  { value: AdjustRecordStatusTypes.processing, label: t("status.processing") },
  { value: AdjustRecordStatusTypes.completed, label: t("status.completed") },
  { value: AdjustRecordStatusTypes.failed, label: t("status.failed") },
];

import i18n from "@/core/plugins/i18n";
import { computed } from "vue";
const t = i18n.global.t;

export const occupations = computed(() => [
  {
    label: t("fields.listedCompanyEmployee"),
    value: "listed-company-employee",
    langKey: "fields.listedCompanyEmployee",
  },
  {
    label: t("fields.unlistedCompanyEmployee"),
    value: "unlisted-company-employee",
    langKey: "fields.unlistedCompanyEmployee",
  },
  {
    label: t("fields.companyOfficer"),
    value: "company-officer",
    langKey: "fields.companyOfficer",
  },
  {
    label: t("fields.privateOrganizationStaff"),
    value: "private-organization-staff",
    langKey: "fields.privateOrganizationStaff",
  },
  {
    label: t("fields.teacher"),
    value: "teacher",
    langKey: "fields.teacher",
  },
  {
    label: t("fields.lawyerAccountantTaxAccountant"),
    value: "lawyer-accountant-tax-accountant",
    langKey: "fields.lawyerAccountantTaxAccountant",
  },
  {
    label: t("fields.doctor"),
    value: "doctor",
    langKey: "fields.doctor",
  },
  {
    label: t("fields.selfEmployed"),
    value: "self-employed",
    langKey: "fields.selfEmployed",
  },
  {
    label: t("fields.freelancer"),
    value: "freelancer",
    langKey: "fields.freelancer",
  },
  {
    label: t("fields.retired"),
    value: "retired",
    langKey: "fields.retired",
  },
  {
    label: t("fields.houseWifeHouseHusband"),
    value: "housewife-househusband",
    langKey: "fields.houseWifeHouseHusband",
  },
  {
    label: t("fields.contractEmployee"),
    value: "contract-employee",
    langKey: "fields.contractEmployee",
  },
  {
    label: t("fields.unemployed"),
    value: "unemployed",
    langKey: "fields.unemployed",
  },
]);

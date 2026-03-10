import i18n from "@/core/plugins/i18n";
import { computed, reactive } from "vue";
import { AccountTypesJp } from "@/core/types/AccountInfos";
const t = i18n.global.t;

export enum accountTypes {
  margin = 0,
  cfdTrading = 1,
  productTrading = 2,
}

export enum accountRoles {
  standard = 0,
  alpha = 1,
}

export const jpAccountTypeMap = {
  [accountRoles.standard]: {
    [accountTypes.margin]: AccountTypesJp.JpSTDFX,
    [accountTypes.cfdTrading]: AccountTypesJp.JpSTDCFD,
    [accountTypes.productTrading]: AccountTypesJp.JpSTDIND,
  },
  [accountRoles.alpha]: {
    [accountTypes.margin]: AccountTypesJp.JpALPFX,
    [accountTypes.cfdTrading]: AccountTypesJp.JpALPCFD,
    [accountTypes.productTrading]: AccountTypesJp.JpALPIND,
  },
};

export const getJpAccountType = (
  role: accountRoles,
  type: accountTypes
): AccountTypesJp => {
  const result = jpAccountTypeMap[role]?.[type];
  if (result === undefined) {
    throw new Error(`Invalid combination of role ${role} and type ${type}`);
  }
  return result;
};

export const investorDistinctionSelection = reactive([
  {
    label: t("fields.generalInvestor"),
    value: 0,
    langKey: "fields.generalInvestor",
  },
  {
    label: t("fields.professionalInvestor"),
    value: 1,
    langKey: "fields.professionalInvestor",
  },
  {
    label: t("fields.specialInvestor"),
    value: 2,
    langKey: "fields.specialInvestor",
  },
]);
export const accountRolesSelection = computed(() => [
  {
    label: t("fields.standard"),
    value: accountRoles.standard,
    langKey: "fields.standard",
  },
  {
    label: t("fields.alpha"),
    value: accountRoles.alpha,
    langKey: "fields.alpha",
  },
]);

export const accountTypesSelection = computed(() => [
  {
    label: t("fields.marginTrading"),
    value: accountTypes.margin,
    langKey: "fields.marginTrading",
  },
  {
    label: t("fields.cfdTrading"),
    value: accountTypes.cfdTrading,
    langKey: "fields.cfdTrading",
  },
  {
    label: t("fields.productTrading"),
    value: accountTypes.productTrading,
    langKey: "fields.productTrading",
  },
]);

export const moneyRange = computed(() => [
  {
    label: t("type.jpMoneyRange.1"),
    value: "1",
    langKey: "type.jpMoneyRange.1",
  },
  {
    label: t("type.jpMoneyRange.2"),
    value: "2",
    langKey: "type.jpMoneyRange.2",
  },
  {
    label: t("type.jpMoneyRange.3"),
    value: "3",
    langKey: "type.jpMoneyRange.3",
  },
  {
    label: t("type.jpMoneyRange.4"),
    value: "4",
    langKey: "type.jpMoneyRange.4",
  },
  {
    label: t("type.jpMoneyRange.5"),
    value: "5",
    langKey: "type.jpMoneyRange.5",
  },
  {
    label: t("type.jpMoneyRange.6"),
    value: "6",
    langKey: "type.jpMoneyRange.6",
  },
  {
    label: t("type.jpMoneyRange.7"),
    value: "7",
    langKey: "type.jpMoneyRange.7",
  },
  {
    label: t("type.jpMoneyRange.8"),
    value: "8",
    langKey: "type.jpMoneyRange.8",
  },
]);

export const mainIncomeSources = computed(() => [
  {
    label: t("fields.businessIncome"),
    value: 0,
    langKey: "fields.businessIncome",
  },
  {
    label: t("fields.realEstateIncome"),
    value: 1,
    langKey: "fields.realEstateIncome",
  },
  {
    label: t("fields.salaryIncome"),
    value: 2,
    langKey: "fields.salaryIncome",
  },
  {
    label: t("fields.interestDividendIncome"),
    value: 3,
    langKey: "fields.interestDividendIncome",
  },
  {
    label: t("fields.pensionIncome"),
    value: 4,
    langKey: "fields.pensionIncome",
  },
  {
    label: t("fields.headOfHouseholdIncome"),
    value: 5,
    langKey: "fields.headOfHouseholdIncome",
  },
  {
    label: t("fields.none"),
    value: 6,
    langKey: "fields.none",
  },
  {
    label: t("fields.other"),
    value: 7,
    langKey: "fields.other",
  },
]);

export const transactionMotives = computed(() => [
  {
    label: t("fields.ourHomepage"),
    value: 0,
    langKey: "fields.ourHomepage",
  },
  {
    label: t("fields.newsPaperMagazine"),
    value: 1,
    langKey: "fields.newsPaperMagazine",
  },
  {
    label: t("fields.takeOver"),
    value: 2,
    langKey: "fields.takeOver",
  },
  {
    label: t("fields.requestForInformation"),
    value: 3,
    langKey: "fields.requestForInformation",
  },
  {
    label: t("fields.webSearch"),
    value: 4,
    langKey: "fields.webSearch",
  },
  {
    label: t("fields.introduction"),
    value: 5,
    langKey: "fields.introduction",
  },
  {
    label: t("fields.other"),
    value: 6,
    langKey: "fields.other",
  },
]);

export const mainFunds = computed(() => [
  {
    label: t("fields.surplusFunds"),
    value: 0,
    langKey: "fields.surplusFunds",
  },
  {
    label: t("fields.determinedUseOfMoney"),
    value: 1,
    langKey: "fields.determinedUseOfMoney",
  },
  {
    label: t("fields.borrowedMoney"),
    value: 2,
    langKey: "fields.borrowedMoney",
  },
  {
    label: t("fields.inheritedMoney"),
    value: 3,
    langKey: "fields.inheritedMoney",
  },
  {
    label: t("fields.retirementMoney"),
    value: 4,
    langKey: "fields.retirementMoney",
  },
  {
    label: t("fields.pensionLivingExpenses"),
    value: 5,
    langKey: "fields.pensionLivingExpenses",
  },
  {
    label: t("fields.other"),
    value: 6,
    langKey: "fields.other",
  },
]);

export const experienceSelections = computed(() => [
  {
    label: t("fields.none"),
    value: 0,
    langKey: "fields.none",
  },
  {
    label: t("fields.lessThanOneYear"),
    value: 1,
    langKey: "fields.lessThanOneYear",
  },
  {
    label: t("fields.lessThanThreeYears"),
    value: 2,
    langKey: "fields.lessThanThreeYears",
  },
  {
    label: t("fields.lessThanFiveYears"),
    value: 3,
    langKey: "fields.lessThanFiveYears",
  },
  {
    label: t("fields.moreThanFiveYears"),
    value: 4,
    langKey: "fields.moreThanFiveYears",
  },
]);

export const investmentPurposes = computed(() => [
  {
    label: t("fields.profitOriented"),
    value: 0,
    langKey: "fields.profitOriented",
  },
  {
    label: t("fields.activeInvestment"),
    value: 1,
    langKey: "fields.activeInvestment",
  },
  {
    label: t("fields.shortTermInvestment"),
    value: 2,
    langKey: "fields.shortTermInvestment",
  },
  {
    label: t("fields.longTermInvestment"),
    value: 3,
    langKey: "fields.longTermInvestment",
  },
  {
    label: t("fields.swapProfit"),
    value: 4,
    langKey: "fields.swapProfit",
  },
  {
    label: t("fields.sellingProfit"),
    value: 5,
    langKey: "fields.sellingProfit",
  },
  {
    label: t("fields.foreignCurrencyAssetHedge"),
    value: 6,
    langKey: "fields.foreignCurrencyAssetHedge",
  },
]);

export const assetManagementPeriodSelection = computed(() => [
  {
    label: t("fields.shortTermOperation"),
    value: 0,
    langKey: "fields.shortTermOperation",
  },
  {
    label: t("fields.mediumTermOperation"),
    value: 1,
    langKey: "fields.mediumTermOperation",
  },
  {
    label: t("fields.longTermOperation"),
    value: 2,
    langKey: "fields.longTermOperation",
  },
  {
    label: t("fields.superLongTermOperation"),
    value: 3,
    langKey: "fields.superLongTermOperation",
  },
]);

export const financialInstiutionTypes = computed(() => [
  {
    label: t("fields.theBank"),
    value: 0,
    langKey: "fields.theBank",
  },
  {
    label: t("fields.shinkinBank"),
    value: 1,
    langKey: "fields.shinkinBank",
  },
  {
    label: t("fields.creditUnion"),
    value: 2,
    langKey: "fields.creditUnion",
  },
  {
    label: t("fields.laborMoney"),
    value: 3,
    langKey: "fields.laborMoney",
  },
  {
    label: t("fields.laborUnion"),
    value: 4,
    langKey: "fields.laborUnion",
  },
  {
    label: t("fields.fisheriesCooperative"),
    value: 5,
    langKey: "fields.fisheriesCooperative",
  },
]);

export const depositTypesSelection = computed(() => [
  {
    label: t("fields.standardAccount"),
    value: 0,
    langKey: "fields.standardAccount",
  },
  {
    label: t("fields.checkingAccount"),
    value: 1,
    langKey: "fields.checkingAccount",
  },
]);

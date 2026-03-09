import { computed } from "vue";
import { useI18n } from "vue-i18n";

/**
 * 财务信息表单选项 Composable
 * 提供收入范围、职位、资金来源、交易经验等选项
 */
export const useFinancialOptions = () => {
  const { t } = useI18n();

  // 收入/投资范围选项
  const range_section = computed(() => ({
    range_1: {
      id: "range_1",
      text: "> $450,000",
      value: "1",
    },
    range_2: {
      id: "range_2",
      text: "$200,000 - $449,999",
      value: "2",
    },
    range_3: {
      id: "range_3",
      text: "$90,000 - $199,999",
      value: "3",
    },
    range_4: {
      id: "range_4",
      text: "$60,000 - 89,999",
      value: "4",
    },
    range_5: {
      id: "range_5",
      text: "$15,000 - 59,999",
      value: "5",
    },
    range_6: {
      id: "range_6",
      text: "< $15,000",
      value: "6",
    },
  }));

  // 职位选项
  const position = computed(() => ({
    pos_1: {
      id: "pos_1",
      text: t("fields.director"),
      value: "director",
    },
    pos_2: {
      id: "pos_2",
      text: t("fields.manager"),
      value: "manager",
    },
    pos_3: {
      id: "pos_3",
      text: t("fields.entryLevel"),
      value: "entry_level",
    },
    pos_4: {
      id: "pos_4",
      text: t("fields.other"),
      value: "other",
    },
  }));

  // 资金来源选项
  const funds = computed(() => ({
    fund_1: {
      id: "fund_1",
      text: t("fields.employment"),
      value: "employment",
    },
    fund_2: {
      id: "fund_2",
      text: t("fields.inheritance"),
      value: "inheritance",
    },
    fund_3: {
      id: "fund_3",
      text: t("fields.savingAndInvestment"),
      value: "saving_and_investment",
    },
    fund_4: {
      id: "fund_4",
      text: t("fields.other"),
      value: "other",
    },
  }));

  // 交易经验问题
  const trading_exp = computed(() => ({
    exp2: {
      id: "exp2",
      text: t("tip.verificationFinancialHaveBankruptcy"),
      show_more: false,
      more_question: t("tip.verificationFinancialIndicateDischarge"),
    },
    exp3: {
      id: "exp3",
      text: t("tip.verificationFinancialAnyPersonOfCommodityOrRegulatory"),
      show_more: false,
      more_question: t("tip.verificationFinancialProvideExchangeOrAgency"),
    },
    exp4: {
      id: "exp4",
      text: t("tip.verificationFinancialAreYouPep"),
      show_more: false,
      more_question: t("tip.yesProvideDetails"),
    },
    exp5: {
      id: "exp5",
      text: t("tip.verificationFinancialAreYouAssociate"),
      show_more: false,
      more_question: t("tip.yesProvideDetails"),
    },
  }));

  return {
    range_section,
    position,
    funds,
    trading_exp,
  };
};

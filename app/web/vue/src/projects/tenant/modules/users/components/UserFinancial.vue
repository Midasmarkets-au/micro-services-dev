<template>
  <div class="w-100 verify-card">
    <div class="">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
          {{ $t("title.financial") }}
        </h2>
      </div>
      <hr />
      <div class="row">
        <div class="col-lg-12 mb-5">
          <label class="form-label fs-6 fw-bold text-dark required">{{
            $t("fields.industry")
          }}</label>
          <Field
            v-model="item.industry"
            tabindex="3"
            class="form-control form-control-lg form-control-solid"
            name="industry"
          >
            <el-select v-model="item.industry" name="industry" size="large">
              <el-option :value="$t('fields.government')">{{
                $t("fields.government")
              }}</el-option>
              <el-option :value="$t('fields.accounting')">{{
                $t("fields.accounting")
              }}</el-option>
              <el-option :value="$t('fields.banking')">{{
                $t("fields.banking")
              }}</el-option>
              <el-option :value="$t('fields.finance')">{{
                $t("fields.finance")
              }}</el-option>
              <el-option :value="$t('fields.insurance')">{{
                $t("fields.insurance")
              }}</el-option>
              <el-option :value="$t('fields.other')">{{
                $t("fields.other")
              }}</el-option>
            </el-select>
          </Field>
          <div class="fv-plugins-message-container">
            <div class="fv-help-block">
              <ErrorMessage name="industry" />
            </div>
          </div>
        </div>

        <div class="col-lg-12 mb-5">
          <label class="form-label fw-bold text-dark fs-6 required">{{
            $t("fields.position")
          }}</label>
          <div class="row">
            <div
              v-for="(v_item, index) in position"
              :key="index"
              class="col-lg-3 mb-3"
            >
              <Field
                v-model="item.position"
                type="radio"
                class="btn-check"
                name="position"
                :value="v_item.value"
                :id="v_item.id"
              />
              <label
                class="btn btn-outline btn-outline-default p-2 d-flex align-items-center"
                :for="v_item.id"
              >
                <span class="d-block fw-semobold text-start">
                  <span class="fw-bold d-block fs-4 mb-2">
                    {{ v_item.text }}
                  </span>
                </span>
              </label>
            </div>

            <ErrorMessage
              name="position"
              class="fv-plugins-message-container invalid-feedback"
            ></ErrorMessage>
          </div>
        </div>

        <div class="col-lg-12 mb-5">
          <label class="form-label fw-bold text-dark fs-6 required">{{
            $t("fields.annualIncome")
          }}</label>
          <div class="row">
            <div
              v-for="(v_item, index) in range_section"
              :key="index"
              class="col-lg-4 mb-3"
            >
              <Field
                v-model="item.income"
                type="radio"
                class="btn-check"
                name="income"
                :value="v_item.value"
                :id="v_item.id + 'income'"
              />
              <label
                class="btn btn-outline btn-outline-default p-2 d-flex align-items-center"
                :for="v_item.id + 'income'"
              >
                <span class="d-block fw-semobold text-start">
                  <span class="fw-bold d-block fs-4 mb-2">
                    {{ v_item.text }}
                  </span>
                </span>
              </label>
            </div>

            <ErrorMessage
              name="income"
              class="fv-plugins-message-container invalid-feedback"
            ></ErrorMessage>
          </div>
        </div>

        <div class="col-lg-12 mb-5">
          <label class="form-label fw-bold text-dark fs-6 required">{{
            $t("fields.valueOfInvestment")
          }}</label>
          <div class="row">
            <div
              v-for="(v_item, index) in range_section"
              :key="index"
              class="col-lg-4 mb-3"
            >
              <Field
                v-model="item.investment"
                type="radio"
                class="btn-check"
                name="investment"
                :value="v_item.value"
                :id="v_item.id + 'investment'"
              />
              <label
                class="btn btn-outline btn-outline-default p-2 d-flex align-items-center"
                :for="v_item.id + 'investment'"
              >
                <span class="d-block fw-semobold text-start">
                  <span class="fw-bold d-block fs-4 mb-2">
                    {{ v_item.text }}
                  </span>
                </span>
              </label>
            </div>

            <ErrorMessage
              name="investment"
              class="fv-plugins-message-container invalid-feedback"
            ></ErrorMessage>
          </div>
        </div>

        <div class="col-lg-12 mb-5">
          <label class="form-label fw-bold text-dark fs-6 required">{{
            $t("fields.howToFundTrading")
          }}</label>
          <div class="row">
            <div
              v-for="(v_item, index) in funds"
              :key="index"
              class="col-lg-3 mb-3"
            >
              <Field
                v-model="item.fund"
                type="checkbox"
                class="btn-check"
                name="fund"
                :value="v_item.value"
                :id="v_item.id"
              />
              <label
                class="btn btn-outline btn-outline-default p-2 d-flex align-items-center"
                :for="v_item.id"
              >
                <span class="d-block fw-semobold text-start">
                  <span class="fw-bold d-block fs-4 mb-2">
                    {{ v_item.text }}
                  </span>
                </span>
              </label>
            </div>

            <div v-if="hasOtherFund" class="col-lg-12 mb-5">
              <label class="form-label fs-6 fw-bold text-dark required">{{
                $t("fields.other")
              }}</label>
              <Field
                v-model="otherFunds"
                tabindex="2"
                type="text"
                name="other-funds"
                autocomplete="off"
              >
                <el-input
                  v-model="otherFunds"
                  tabindex="2"
                  type="text"
                  name="other-funds"
                  :placeholder="$t('tip.useCommaSeparate')"
                  autocomplete="off"
                  size="large"
                  @blur="handleOtherFundsInputBlurred"
                />
              </Field>
            </div>

            <ErrorMessage
              name="fund"
              class="fv-plugins-message-container invalid-feedback"
            ></ErrorMessage>
          </div>
        </div>
      </div>
    </div>
  </div>
  <div class="w-100 my-5 verify-card">
    <div class="">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
          {{ $t("title.tradingBackground") }}
        </h2>
      </div>
      <hr />

      <div class="row">
        <div class="col-lg-12 mb-5">
          <label class="form-label fw-bold text-dark fs-6 required">{{
            $t("tip.verificationFinancialHaveEducation")
          }}</label>
          <div class="row">
            <div class="col-6 col-lg-2">
              <Field
                v-model="item.bg1"
                type="radio"
                class="btn-check"
                name="bg1"
                value="0"
                id="bg1_yes"
              />
              <label
                class="btn btn-outline btn-outline-default p-3 d-flex align-items-center"
                for="bg1_yes"
              >
                <span class="d-block fw-semobold text-start">
                  <span class="fw-bold d-block fs-4 mb-2">
                    {{ $t("action.yes") }}
                  </span>
                </span>
              </label>
            </div>

            <!-- --------------------------------------------- -->

            <div class="col-6 col-lg-2">
              <Field
                v-model="item.bg1"
                type="radio"
                class="btn-check"
                name="bg1"
                value="1"
                id="bg1_no"
              />
              <label
                class="btn btn-outline btn-outline-default p-3 d-flex align-items-center"
                for="bg1_no"
              >
                <span class="d-block fw-semobold text-start">
                  <span class="fw-bold d-block fs-4 mb-2">{{
                    $t("action.no")
                  }}</span>
                </span>
              </label>
            </div>

            <ErrorMessage
              name="bg1"
              class="fv-plugins-message-container invalid-feedback"
            ></ErrorMessage>
          </div>
        </div>
        <div class="col-lg-12 mb-5">
          <label class="form-label fw-bold text-dark fs-6 required">{{
            $t("tip.verificationFinancialHaveTradeAccount")
          }}</label>
          <div class="row">
            <div class="col-6 col-lg-2">
              <Field
                v-model="item.bg2"
                type="radio"
                class="btn-check"
                name="bg2"
                value="0"
                id="bg2_yes"
              />
              <label
                class="btn btn-outline btn-outline-default p-3 d-flex align-items-center"
                for="bg2_yes"
              >
                <span class="d-block fw-semobold text-start">
                  <span class="fw-bold d-block fs-4 mb-2">
                    {{ $t("action.yes") }}
                  </span>
                </span>
              </label>
            </div>

            <div class="col-6 col-lg-2">
              <Field
                v-model="item.bg2"
                type="radio"
                class="btn-check"
                name="bg2"
                value="1"
                id="bg2_no"
              />
              <label
                class="btn btn-outline btn-outline-default p-3 d-flex align-items-center"
                for="bg2_no"
              >
                <span class="d-block fw-semobold text-start">
                  <span class="fw-bold d-block fs-4 mb-2">{{
                    $t("action.no")
                  }}</span>
                </span>
              </label>
            </div>

            <ErrorMessage
              name="bg2"
              class="fv-plugins-message-container invalid-feedback"
            ></ErrorMessage>
          </div>
        </div>
      </div>
    </div>
  </div>
  <div class="w-100 verify-card">
    <div class="">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark ms-3">
          {{ $t("title.relevantTradingExperience") }}
        </h2>
      </div>
      <hr />

      <div class="row">
        <div class="col-lg-12 mb-5">
          <label class="form-label fw-bold text-dark fs-6 required">
            {{ $t("tip.verificationFinancialAreYouBcrEmployee") }}
          </label>
          <div class="row">
            <div class="col-6 col-lg-2">
              <Field
                v-model="item.exp1"
                type="radio"
                class="btn-check"
                name="exp1"
                value="0"
                id="exp1_true"
                @click="show_exp1_more = true"
              />
              <label
                class="btn btn-outline btn-outline-default p-3 d-flex align-items-center"
                for="exp1_true"
              >
                <span class="d-block fw-semobold text-start">
                  <span class="fw-bold d-block fs-4 mb-2">
                    {{ $t("action.yes") }}
                  </span>
                </span>
              </label>
            </div>

            <div class="col-6 col-lg-2">
              <Field
                v-model="item.exp1"
                type="radio"
                class="btn-check"
                name="exp1"
                value="1"
                id="exp1_false"
                @click="show_exp1_more = false"
              />
              <label
                class="btn btn-outline btn-outline-default p-3 d-flex align-items-center"
                for="exp1_false"
              >
                <span class="d-block fw-semobold text-start">
                  <span class="fw-bold d-block fs-4 mb-2">{{
                    $t("action.no")
                  }}</span>
                </span>
              </label>
            </div>

            <ErrorMessage
              name="exp1"
              class="fv-plugins-message-container invalid-feedback"
            ></ErrorMessage>
          </div>
        </div>
        <div v-if="show_exp1_more || item.exp1 == '0'" class="col-lg-12 mb-5">
          <label class="form-label fs-6 fw-bold text-dark required">{{
            $t("fields.employer")
          }}</label>
          <Field
            v-model="item.exp1_employer"
            tabindex="2"
            type="text"
            name="exp1_employer"
            autocomplete="off"
          >
            <el-input
              v-model="item.exp1_employer"
              tabindex="2"
              type="text"
              name="exp1_employer"
              size="large"
              autocomplete="off"
            />
          </Field>
          <div class="fv-plugins-message-container">
            <div class="fv-help-block">
              <ErrorMessage name="exp1_employer" />
            </div>
          </div>
        </div>
        <div v-if="show_exp1_more || item.exp1 == '0'" class="col-lg-12 mb-5">
          <label class="form-label fs-6 fw-bold text-dark required">{{
            $t("fields.position")
          }}</label>
          <Field
            v-model="item.exp1_position"
            tabindex="2"
            type="text"
            name="exp1_position"
            autocomplete="off"
          >
            <el-input
              v-model="item.exp1_position"
              tabindex="2"
              type="text"
              name="exp1_position"
              autocomplete="off"
              size="large"
            />
          </Field>
          <div class="fv-plugins-message-container">
            <div class="fv-help-block">
              <ErrorMessage name="exp1_position" />
            </div>
          </div>
        </div>
        <div v-if="show_exp1_more || item.exp1 == '0'" class="col-lg-12 mb-5">
          <label class="form-label fs-6 fw-bold text-dark required">{{
            $t("fields.remuneration")
          }}</label>
          <Field
            v-model="item.exp1_remuneration"
            tabindex="2"
            type="text"
            name="exp1_remuneration"
            autocomplete="off"
          >
            <el-input
              v-model="item.exp1_remuneration"
              tabindex="2"
              type="text"
              name="exp1_remuneration"
              autocomplete="off"
              size="large"
            />
          </Field>
          <div class="fv-plugins-message-container">
            <div class="fv-help-block">
              <ErrorMessage name="exp1_remuneration" />
            </div>
          </div>
        </div>
        <div
          v-for="(v_item, index) in trading_exp"
          :key="index"
          class="col-lg-12 mb-5"
        >
          <label class="form-label fw-bold text-dark fs-6 required">{{
            v_item.text
          }}</label>
          <div class="row">
            <div class="col-6 col-lg-2">
              <Field
                v-model="item[v_item.id]"
                type="radio"
                class="btn-check"
                :name="v_item.id"
                value="0"
                :id="v_item.id + '_true'"
                @click="v_item.show_more = true"
              />
              <label
                class="btn btn-outline btn-outline-default p-3 d-flex align-items-center"
                :for="v_item.id + '_true'"
              >
                <span class="d-block fw-semobold text-start">
                  <span class="fw-bold d-block fs-4 mb-2">
                    {{ $t("action.yes") }}</span
                  >
                </span>
              </label>
            </div>

            <div class="col-6 col-lg-2">
              <Field
                v-model="item[v_item.id]"
                type="radio"
                class="btn-check"
                :name="v_item.id"
                value="1"
                :id="v_item.id + '_false'"
                @click="v_item.show_more = false"
              />
              <label
                class="btn btn-outline btn-outline-default p-3 d-flex align-items-center"
                :for="v_item.id + '_false'"
              >
                <span class="d-block fw-semobold text-start">
                  <span class="fw-bold d-block fs-4 mb-2">{{
                    $t("action.no")
                  }}</span>
                </span>
              </label>
            </div>

            <ErrorMessage
              :name="v_item.id"
              class="fv-plugins-message-container invalid-feedback"
            ></ErrorMessage>
          </div>

          <div
            v-if="v_item.show_more || item[v_item.id] == '0'"
            class="col-lg-12 mt-10 ms-3"
          >
            <label class="form-label fs-6 fw-bold text-dark required">{{
              v_item.more_question
            }}</label>
            <Field
              v-model="item[v_item.id + '_more']"
              tabindex="2"
              type="text"
              :name="v_item.id + '_more'"
              autocomplete="off"
            >
              <el-input
                v-model="item[v_item.id + '_more']"
                tabindex="2"
                type="text"
                :name="v_item.id + '_more'"
                autocomplete="off"
                size="large"
              />
            </Field>
            <div class="fv-plugins-message-container">
              <div class="fv-help-block">
                <ErrorMessage :name="v_item.id + '_more'" />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <!--    <div class="btn btn-primary">{{ item }}</div>-->
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch, onMounted } from "vue";
import { ErrorMessage, Field } from "vee-validate";
import { useI18n } from "vue-i18n";

const isSubmit = ref(false);
const { t } = useI18n();

const show_exp1_more = ref(false);
const hasOtherFund = ref(false);
const otherFunds = ref<string>("");
const handleOtherFundsInputBlurred = () => {
  const otherFundsList = otherFunds.value
    .split(",")
    .map((x) => x.trim())
    .filter((y) => y !== "");
  item.value.fund = [...new Set([...item.value.fund, ...otherFundsList])];
};

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

const props = defineProps<{
  verificationDetails: any;
}>();

const item = ref<any>({
  ...props.verificationDetails?.financial,
});

const initHasOtherFund = () => {
  const defaultFunds = Object.keys(funds.value).map(
    (x) => funds.value[x].value
  );
  hasOtherFund.value = item.value.fund?.some(
    (x: string) => x === funds.value.fund_4.value
  );
  if (!hasOtherFund.value) {
    otherFunds.value = "";
    item.value.fund = item.value.fund?.filter((x: string) =>
      defaultFunds.includes(x)
    );
    return;
  }
  otherFunds.value = item.value.fund
    ?.filter((x: string) => !defaultFunds.includes(x))
    .join(", ");
};

onMounted(() => {
  initHasOtherFund();
});

watch(
  () => props.verificationDetails,
  () => {
    item.value = {
      ...props.verificationDetails?.financial,
    };
  }
);
</script>

<style scoped></style>

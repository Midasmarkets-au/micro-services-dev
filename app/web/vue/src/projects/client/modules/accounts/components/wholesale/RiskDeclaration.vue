<template>
  <div class="card wholesale-process-card">
    <div class="card-body p-3 p-md-0">
      <div
        class="mb-7 fs-3 fw-bold"
        style="
          text-decoration: underline;
          text-decoration-color: #ffce00;
          text-decoration-thickness: 3px;
          text-underline-offset: 5px;
        "
      >
        {{ $t("wholesale.riskTitle_1") }}
      </div>

      <h5>{{ $t("wholesale.riskTitle_2") }}</h5>

      <div class="mt-7" v-html="$t('wholesale.riskContent_1')"></div>

      <div class="mt-7" v-html="$t('wholesale.riskContent_2')"></div>

      <div class="mt-7" v-html="$t('wholesale.riskContent_3')"></div>

      <div class="mt-7">
        {{ $t("wholesale.riskContent_4") }}
        <a
          target="_blank"
          :href="
            getWholeSaleDocs(
              wholeSaleDocs.wholesaleClientDisclosureNotice.title
            )
          "
          ><strong>{{ $t("wholesale.riskContent_4_1") }}</strong></a
        >
      </div>

      <div class="mt-7" v-html="$t('wholesale.riskContent_5')"></div>

      <div class="mt-7" v-html="$t('wholesale.riskContent_6')"></div>

      <div class="mt-7" v-html="$t('wholesale.riskContent_7')"></div>

      <div class="mt-7" v-html="$t('wholesale.riskContent_8')"></div>

      <div class="mt-7" v-html="$t('wholesale.riskContent_9')"></div>

      <div
        class="d-flex align-items-center mt-13"
        style="padding: 20px"
        :style="
          checkBoxError
            ? 'border: 2px solid #b52025'
            : 'border: 2px solid lightgray'
        "
      >
        <!--begin::Checkbox-->
        <div class="form-check form-check-custom form-check-solid mx-5">
          <Field
            v-model="checkbox"
            class="form-check-input"
            type="checkbox"
            :value="true"
            name="checkbox"
          />
        </div>
        <!--end::Checkbox-->
        <div>
          {{ $t("wholesale.riskContent_10") }}
        </div>
      </div>

      <div
        class="mt-13 mb-7 fs-3 fw-bold"
        style="
          text-decoration: underline;
          text-decoration-color: #ffce00;
          text-decoration-thickness: 3px;
          text-underline-offset: 5px;
        "
      >
        {{ $t("wholesale.riskContent_11") }}
      </div>

      <div>
        {{ $t("wholesale.riskContent_12") }}
      </div>

      <div class="mt-7">{{ $t("wholesale.riskContent_13") }}</div>

      <div class="mt-5 ms-9">
        <div class="d-flex">
          <div class="bullet"></div>
          <div>
            {{ $t("wholesale.riskContent_14") }}
          </div>
        </div>
        <div class="d-flex mt-3">
          <div class="bullet"></div>
          <div>
            {{ $t("wholesale.riskContent_15") }}
          </div>
        </div>
        <div class="d-flex mt-3">
          <div class="bullet"></div>
          <div>
            {{ $t("wholesale.riskContent_16") }}
            me.
          </div>
        </div>
        <div class="d-flex mt-3">
          <div class="bullet"></div>
          <div>
            {{ $t("wholesale.riskContent_17") }}
          </div>
        </div>
      </div>

      <div class="mt-7">{{ $t("wholesale.riskContent_18") }}</div>

      <div class="mt-5 ms-9">
        <div class="d-flex">
          <div class="bullet"></div>
          <div>{{ $t("wholesale.riskContent_19") }}</div>
        </div>
        <div class="d-flex mt-3">
          <div class="bullet"></div>
          <div>
            {{ $t("wholesale.riskContent_20") }}
          </div>
        </div>
        <div class="d-flex mt-3">
          <div class="bullet"></div>
          <div>
            {{ $t("wholesale.riskContent_21") }}
          </div>
        </div>
      </div>

      <div class="mt-7">
        {{ $t("wholesale.riskContent_22") }}
      </div>
      <div class="mt-5 ms-9">
        <div class="d-flex">
          <div class="bullet"></div>
          <div>{{ $t("wholesale.riskContent_23") }}</div>
        </div>
        <div class="d-flex mt-3">
          <div class="bullet"></div>
          <div>{{ $t("wholesale.riskContent_24") }}</div>
        </div>
        <div class="d-flex mt-3">
          <div class="bullet"></div>
          <div>
            {{ $t("wholesale.riskContent_25") }}
          </div>
        </div>
      </div>

      <div class="mt-9" v-html="$t('wholesale.riskContent_26')"></div>

      <div
        v-if="checkBoxError"
        style="color: #b52025; text-align: right; margin-top: 30px"
      >
        {{ $t("wholesale.error_1") }}
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import { Field } from "vee-validate";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import AccountService from "../../services/AccountService";
import { useStore } from "@/store";
import { getLanguage } from "@/core/types/LanguageTypes";
import { wholeSaleDocs, getWholeSaleDocs } from "@/core/data/bcrDocs";
const props = defineProps<{
  data?: object;
  accountUid?: number;
  account?: any;
}>();

const emits = defineEmits(["saved", "hasError"]);

const checkbox = ref(false);
const checkBoxError = ref(false);

const handleStepSubmit = async () => {
  if (checkbox.value) {
    try {
      await AccountService.postWholesaleApplication({
        accountUid: props.accountUid,
        accountNumber: props.account.tradeAccount.accountNumber,
        request: props.data,
      });

      emits("saved", 4);
    } catch (error) {
      MsgPrompt.error(error);
      emits("hasError");
    }
  } else {
    emits("hasError");
    checkBoxError.value = true;
  }
};

defineExpose({
  handleStepSubmit,
});
</script>

<style scoped>
.bullet {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background-color: #000;
  margin-right: 10px;
  margin-top: 5px;
}
</style>

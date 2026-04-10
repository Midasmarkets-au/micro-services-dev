<template>
  <div class="w-100 card">
    <div class="card-header">
      <div class="card-title">
        {{ $t("title.agreement") }}
      </div>
    </div>
    <div class="card-body">
      <el-form-item prop="consent_1" class="mb-5">
        <el-checkbox
          v-model="formData.consent_1"
          :label="$t('tip.verificationAgreementAcknowledgeTerms')"
          size="large"
          style="color: #868d98"
        />
      </el-form-item>
      <div class="agreementList">
        <ul class="fw-bold">
          <li v-for="(item, index) in filteredDocs" :key="index">
            <a
              :href="item.url"
              target="_blank"
              class="cursor-pointer text-dark text-hover-primary"
            >
              {{ $t(`title.${item.title}`) }}
            </a>
          </li>
        </ul>
      </div>

      <div class="mt-5">
        <h4>{{ $t("title.electronicIdentityVerification") }}*</h4>
        <p class="text-gray mt-5 mb-10">
          {{ $t("tip.verificationAgreementElectronicIdentityVerification1") }}
          <span>MDM Co Ltd</span>
          {{ $t("tip.verificationAgreementElectronicIdentityVerification2") }}
        </p>

        <el-form-item prop="consent_2">
          <el-radio
            v-model="formData.consent_2"
            :label="true"
            class="identifyOption"
            style="border-radius: 10px; box-sizing: border-box; width: 100%"
            >{{ $t("tip.verificationAgreementConsentPassingInfo") }}
          </el-radio>
        </el-form-item>

        <!-- <el-form-item prop="consent_2">
          <el-radio
            v-model="formData.consent_2"
            :label="false"
            class="identifyOption"
            style="
              border-radius: 10px;
              box-sizing: border-box;
              width: 100%;
              padding: 20px 30px;
            "
          >
            <div class="ps-1">
              <p>
                {{ $t("tip.verificationAgreementNotConsentPassingInfo") }}
              </p>
              <div style="color: #f7291a" class="mt-3">
                {{ $t("tip.verificationAgreementPrepareId") }}
              </div>
            </div>
          </el-radio>
        </el-form-item> -->
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, inject, onMounted } from "vue";
import { getBviDocs, bviVerficationDocs } from "@/core/data/bcrDocs";

const items = inject<any>("items");
const formData = inject<any>("formData");
const showIbDocuments = inject<any>("showIbDocuments");

const item = ref<any>(items?.value?.data?.agreement || {});
const filteredDocs = ref<any>([]);

const filterDocs = async () => {
  Object.entries(bviVerficationDocs).forEach(([key, value]) => {
    if (getBviDocs(value.title, showIbDocuments.value) !== false) {
      filteredDocs.value.push({
        title: key,
        url: getBviDocs(value.title, showIbDocuments.value),
      });
    }
  });
};

onMounted(async () => {
  await filterDocs();
  console.log("formData", formData.value);
  console.log("item", item.value);
  formData.value = item.value;
  formData.value.consent_1 = item.value.consent_1 ? item.value.consent_1 : true;
  formData.value.consent_2 = item.value.consent_2 ? item.value.consent_2 : true;
});
</script>
<style scoped>
.agreementList {
  margin-left: 35px;
  margin-top: 20px;
}
.identifyOption {
  padding: 30px;
  background-color: #fafbfd;
  &.is-checked {
    border: 1px solid #000f32;
  }
}
@media (max-width: 768px) {
  .verify-card {
    padding-left: 20px;
    padding-right: 20px;
  }
  .agreementList {
    margin-left: 0px;
  }
  .identifyOption {
    padding: 20px 10px;
  }
}
::v-deep .el-checkbox {
  height: auto;
}
::v-deep .el-checkbox__label {
  white-space: normal;
  line-height: 1.2;
  color: #868d98;
  font-weight: normal;
  @media (max-width: 768px) {
    font-size: 14px;
  }
}

::v-deep .el-checkbox__inner {
  width: 20px !important;
  height: 20px !important;
}

::v-deep .el-checkbox__inner::after {
  box-sizing: content-box;
  content: "";
  border: 1px solid var(--el-checkbox-checked-icon-color);
  border-left: 0;
  border-top: 0;
  height: 15px;
  left: 5px;
  position: absolute;
  top: 0px;
  transform: rotate(45deg) scaleY(0);
  width: 7px;
  transition: transform 0.15s ease-in 50ms;
  transform-origin: center;
}

::v-deep .el-radio {
  height: auto;
  white-space: normal;
  cursor: pointer;
}
::v-deep .el-radio__label {
  cursor: auto;
  line-height: 1.2;
  color: #868d98;
}

::v-deep .el-radio__input.is-checked + .el-radio__label {
  color: #868d98 !important;
}
::v-deep .el-checkbox__input.is-checked + .el-checkbox__label {
  color: #868d98 !important;
}
/* ::v-deep .el-checkbox {
  --el-checkbox-checked-text-color: #606266 !important;
  --el-checkbox-checked-input-border-color: #ffce00 !important;
  --el-checkbox-checked-bg-color: #ffce00 !important;
  --el-checkbox-checked-icon-color: #000000 !important;
  --el-checkbox-input-border-color-hover: #ffce00 !important;
}

::v-deep .el-radio__input {
  --el-radio-input-border-color-hover: #ffce00 !important;
}

::v-deep .el-radio__input.is-checked + .el-radio__label {
  color: #868d98 !important;
}

::v-deep .el-radio__input.is-checked .el-radio__inner {
  border-color: #ffce00;
  background-color: #ffce00;
}
::v-deep .el-radio__inner {
  width: 25px;
  height: 25px;
} */
/* 
::v-deep .el-radio__input.is-checked .el-radio__inner::after {
  background-color: black;
  width: 12px;
  height: 12px;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
} */
</style>

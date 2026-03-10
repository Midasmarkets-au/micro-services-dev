<template>
  <div class="card">
    <div class="card-body">
      <div class="pb-3">
        <h2
          class="fw-bold d-flex align-items-center text-dark ms-3 mt-3 mt-lg-0"
        >
          {{ $t("title.agreement") }}
        </h2>
      </div>
      <hr />
      <div class="box-container">
        <div class="row bottom">
          <div class="col-12 col-md-10 left">
            {{ $t("type.jpAgreement.check1") }}
          </div>
          <div class="col-12 col-md-2 right">
            <el-form-item
              prop="check_1"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="formData.check_1">{{
                $t("fields.iAccept")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>

        <!-- account type margin -->
        <div
          class="row bottom"
          v-if="item.accountTypes.includes(accountTypes.margin)"
        >
          <div class="col-12 col-md-10 left">
            <div class="mb-1">
              <a
                :href="jpDocs.foreignExchangeMarginTrading.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.foreignExchangeMarginTrading.title }}</a
              >
            </div>
            <div class="mb-1">
              <a
                :href="jpDocs.foreignExchangeTermsAndConditions.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.foreignExchangeTermsAndConditions.title }}</a
              >
            </div>
            <div class="mb-1">
              <a
                :href="jpDocs.foreginExchangeTradingManual.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.foreginExchangeTradingManual.title }}</a
              >
            </div>
          </div>
          <div class="col-12 col-md-2 right">
            <el-form-item
              prop="check_2"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="formData.check_2">{{
                $t("fields.iAccept")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>

        <!-- account type cfd -->
        <div
          class="row bottom"
          v-if="item.accountTypes.includes(accountTypes.cfdTrading)"
        >
          <div class="col-12 col-md-10 left">
            <div class="mb-1">
              <a
                :href="jpDocs.foreignExchangeMarginTrading.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.foreignExchangeMarginTrading.title }}</a
              >
            </div>
            <div class="mb-1">
              <a
                :href="jpDocs.cfdTradingTermsAndConditions.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.cfdTradingTermsAndConditions.title }}</a
              >
            </div>
            <div class="mb-1">
              <a
                :href="jpDocs.cfdTradingManual.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.cfdTradingManual.title }}</a
              >
            </div>
          </div>
          <div class="col-12 col-md-2 right">
            <el-form-item
              prop="check_3"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="formData.check_3">{{
                $t("fields.iAccept")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>

        <!-- account type cfd_2 -->
        <div
          class="row bottom"
          v-if="item.accountTypes.includes(accountTypes.productTrading)"
        >
          <div class="col-12 col-md-10 left">
            <div class="mb-1">
              <a
                :href="jpDocs.productCfdTrading.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.productCfdTrading.title }}</a
              >
            </div>
            <div class="mb-1">
              <a
                :href="jpDocs.commodityCfdTradingTermsAndConditions.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.commodityCfdTradingTermsAndConditions.title }}</a
              >
            </div>
            <div class="mb-1">
              <a
                :href="jpDocs.productCfdTradingManual.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.productCfdTradingManual.title }}</a
              >
            </div>
            <div class="mb-1">
              <a
                :href="jpDocs.productCfdTradingForCorporations.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.productCfdTradingForCorporations.title }}</a
              >
            </div>
          </div>
          <div class="col-12 col-md-2 right">
            <el-form-item
              prop="check_4"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="formData.check_4">{{
                $t("fields.iAccept")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>
        <div class="row bottom">
          <div class="col-12 col-md-10 left">
            <div class="mb-1">
              <a
                :href="jpDocs.agreementRegardingElectronicDelivery.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.agreementRegardingElectronicDelivery.title }}</a
              >
            </div>
            <div class="mb-1">
              <a
                :href="jpDocs.personalInformationProtectionDeclaration.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.personalInformationProtectionDeclaration.title }}</a
              >
            </div>
            <div class="mb-1">
              <a
                :href="jpDocs.agreementForAntiSocialForce.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.agreementForAntiSocialForce.title }}</a
              >
            </div>
            <div class="mb-1">
              <a
                :href="jpDocs.notificationOfNotFallingUnderForeignPeps.src"
                target="_blank"
                class="pdf"
              >
                {{ jpDocs.notificationOfNotFallingUnderForeignPeps.title }}</a
              >
            </div>
          </div>
          <div class="col-12 col-md-2 right">
            <el-form-item
              prop="check_5"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="formData.check_5">{{
                $t("fields.iAccept")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>

        <div class="row bottom">
          <div class="col-12 col-md-10 left">
            <div>①{{ $t("type.jpAgreement.check2Title") }}</div>
            <div>
              {{ $t("type.jpAgreement.check2") }}
            </div>
          </div>
          <div class="col-12 col-md-2 right">
            <el-form-item
              prop="check_6"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="formData.check_6">{{
                $t("action.yes")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>
        <div class="row bottom">
          <div class="col-12 col-md-10 left">
            <div>②{{ $t("type.jpAgreement.check3Title") }}</div>
            <div>
              {{ $t("type.jpAgreement.check3") }}
            </div>
          </div>
          <div class="col-12 col-md-2 right">
            <el-form-item
              prop="check_7"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="formData.check_7">{{
                $t("action.yes")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>
        <div class="row bottom">
          <div class="col-12 col-md-10 left">
            <div>③{{ $t("type.jpAgreement.check4Title") }}</div>
            <div>
              {{ $t("type.jpAgreement.check4") }}
            </div>
          </div>
          <div class="col-12 col-md-2 right">
            <el-form-item
              prop="check_8"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="formData.check_8">{{
                $t("action.yes")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>
        <div class="row bottom">
          <div class="col-12 col-md-10 left">
            <div>④{{ $t("type.jpAgreement.check5Title") }}</div>
            <div>
              {{ $t("type.jpAgreement.check5") }}
            </div>
          </div>
          <div class="col-12 col-md-2 right">
            <el-form-item
              prop="check_9"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="formData.check_9">{{
                $t("action.yes")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>
        <div class="row bottom">
          <div class="col-12 col-md-10 left">
            <div>⑤{{ $t("type.jpAgreement.check6Title") }}</div>
            <div>
              {{ $t("type.jpAgreement.check6") }}
            </div>
          </div>
          <div class="col-12 col-md-2 right">
            <el-form-item
              prop="check_10"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="formData.check_10">{{
                $t("action.yes")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>
        <div class="row">
          <div class="col-12 col-md-10 left">
            <div class="text-decoration-underline">
              {{ $t("type.jpAgreement.check7") }}
            </div>
          </div>
          <div class="col-12 col-md-2 right">
            <el-form-item
              prop="check_11"
              class="d-flex align-items-center justify-content-center"
            >
              <el-checkbox v-model="formData.check_11">{{
                $t("action.yes")
              }}</el-checkbox>
            </el-form-item>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, inject, onMounted } from "vue";
import { accountTypes } from "@/core/types/jp/verificationFinancial";
import { jpDocs } from "@/core/data/bcrDocs";
const formData = inject<any>("formData");
const items = inject<any>("items");
const item = ref<any>(items?.value?.data?.financial || {});
const agreements = ref<any>(items.value?.data?.agreement || {});
onMounted(() => {
  formData.value = agreements.value;
});
</script>
<style lang="css" scoped>
.box-container {
  border: 1px solid #cccccc;
  border-radius: 10px;
  box-sizing: border-box;
}
.bottom {
  border-bottom: 1px solid #cccccc;
}
.left {
  border-right: 1px solid #cccccc;
  padding: 20px;
}

@media (max-width: 768px) {
  .left {
    padding: 20px;
    border-right: none;
  }
}

.right {
  text-align: center;
  margin: auto;
}

.pdf {
  color: #000;
  text-decoration: underline;
  cursor: pointer;
}
.pdf:hover {
  color: #409eff;
  text-decoration: underline;
}

:deep .el-form-item__content {
  justify-content: center;
}

@media (max-width: 768px) {
  :deep .card-body {
    padding: 0rem 0rem;
  }
}
</style>

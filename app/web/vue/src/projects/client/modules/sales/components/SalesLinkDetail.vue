<template>
  <div
    class="modal fade"
    id="kt_modal_iblibk_detail"
    tabindex="-1"
    aria-hidden="true"
    ref="SalesLinkDetailModalRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-750px">
      <div class="modal-content">
        <div class="modal-header" id="kt_modal_new_address_header">
          <h2 class="fs-2">{{ $t("title.rebateSettings") }}</h2>

          <div data-bs-dismiss="modal">
            <span class="svg-icon svg-icon-1">
              <inline-svg src="/images/icons/arrows/arr061.svg" />
            </span>
          </div>
        </div>
        <div v-if="isLoading">
          <table class="table align-middle table-row-bordered gy-5">
            <tbody>
              <LoadingRing />
            </tbody>
          </table>
        </div>

        <!-- ------------------------------------------------------------------ -->
        <div v-if="!isLoading" style="padding: 20px">
          <div v-if="salesLinkDetail == null">
            <table class="table align-middle table-row-bordered gy-5">
              <tbody>
                <NoDataBox />
              </tbody>
            </table>
          </div>
          <!-- STEP 1 -->
          <div v-else>
            <!-- <div class="row">
              <div class="col-6">
                <div class="d-flex align-items-center">
                  <div class="dot me-3"></div>
                  <span class="step me-3">{{ $t("fields.step1") }}</span>
                  <span class="stepContent">{{ $t("tip.nameYourLink") }}</span>
                </div>
                <Field
                  class="form-control form-control-solid w-300px h-55px mt-5"
                  :placeholder="$t('tip.pleaseInput')"
                  name="name"
                  v-model="salesLinkDetail.name"
                  disabled
                />
              </div>
              <div class="col-6">
                <div class="d-flex align-items-center">
                  <div class="dot me-3"></div>
                  <span class="step me-3">{{ $t("fields.step2") }}</span>
                  <span class="stepContent">{{
                    $t("tip.nameYourReferCode")
                  }}</span>
                </div>
                <Field
                  class="form-control form-control-solid w-300px h-55px mt-5"
                  :placeholder="$t('tip.pleaseInput')"
                  name="code"
                  v-model="salesLinkDetail.code"
                  disabled
                />
              </div>
            </div> -->

            <!-- STEP 3 -->
            <!-- <div class="d-flex align-items-center mt-15">
              <div class="dot me-3"></div>
              <span class="step me-3">{{ $t("fields.step3") }}</span>
              <span class="stepContent">{{ $t("action.chooseLanguage") }}</span>
            </div>
            <Field
              name="language"
              class="form-select form-select-solid mt-5 IB-rebate-link-select"
              as="select"
              v-model="salesLinkDetail.summary.language"
              disabled
            >
              <option value="en-us">English</option>
              <option value="zh-cn">Simplify Chinese (简体中文)</option>
              <option value="zh-hk">Traditional Chinese (繁体中文)</option>
              <option value="vi-vn">Vietnamese (Tiếng Việt Nam)</option>
              <option value="th-th">Thai (ภาษาไทย)</option>
              <option value="jp-jp">Japanese (日本語)</option>
              <option value="id-id">Indonesian (Bahasa Indonesia)</option>
              <option value="ms-my">Malay (Bahasa Melayu)</option>
            </Field> -->

            <!-- STEP 4 -->
            <!-- <div class="d-flex align-items-center mt-15">
              <div class="dot me-3"></div>
              <span class="step me-3">{{ $t("fields.step4") }}</span>
              <span class="stepContent">{{
                $t("tip.selectAccountTypeUnderLink")
              }}</span>
            </div> -->
            <div class="mt-5 mb-15">
              <Field
                v-model="salesLinkDetail.serviceType"
                class="form-check-input widget-9-check me-3"
                type="radio"
                name="serviceType"
                value="200"
                disabled
              />
              <label class="me-9" for="accountTypeIB">{{
                $t("title.ib")
              }}</label>
              <Field
                v-model="salesLinkDetail.serviceType"
                class="form-check-input widget-9-check me-3"
                type="radio"
                name="serviceType"
                value="400"
                disabled
              />
              <label class="me-9" for="accountTypeClient">{{
                $t("fields.client")
              }}</label>
            </div>

            <!-- STEP 4 -->
            <div class="d-flex align-items-center mt-7">
              <div class="dot me-3"></div>
              <span class="step me-3">{{ $t("fields.step5") }}</span>
              <span class="stepContent d-flex"
                >{{ $t("tip.selectAccountTypeTopLeft") }}
              </span>
            </div>

            <div class="mt-3">
              <div
                v-if="salesLinkDetail.serviceType == ReferralServiceType.Broker"
                class="row"
                style="position: relative"
              >
                <div
                  class="col-12 mb-7"
                  v-for="(schema, index) in salesLinkDetail.summary.schema"
                  :key="index"
                  style="
                    border-radius: 10px;
                    box-shadow: rgba(0, 0, 0, 0.16) 0px 1px 4px;
                    padding: 10px;
                  "
                >
                  <div class="d-flex justify-content-between">
                    <div class="d-flex align-items-center">
                      <input
                        :id="schema.accountType"
                        class="form-check-input widget-9-check me-3"
                        type="checkbox"
                        :name="schema.accountType"
                        checked
                        disabled
                      />
                      <div class="fs-4 text-center">
                        <label for="rebateStdAccount">{{
                          $t("type.account." + schema.accountType)
                        }}</label>
                      </div>
                    </div>
                    <div class="badge text-bg-primary">
                      {{
                        schema.optionName == "alpha"
                          ? $t("type.shortAccount.alpha")
                          : schema.optionName
                      }}
                    </div>
                  </div>

                  <hr />
                  <div class="row">
                    <div
                      class="col-6 col-lg-2 mb-1"
                      v-for="(item, index) in schema.items"
                      :key="index"
                    >
                      <div>
                        {{ $t("type.clientSymbolCategory." + item.cid) }}
                      </div>
                      <el-input
                        class="w-100% h-35px mt-1 mb-1"
                        v-model="item.r"
                        disabled
                      />
                    </div>
                  </div>

                  <div v-if="schema.allowPips.length > 0" class="row mt-3">
                    <label>{{ $t("fields.availablePips") }} </label>
                    <el-checkbox-group
                      v-model="schema.allowPips"
                      size="large"
                      disabled
                    >
                      <el-checkbox
                        v-for="(item, index) in schema.allowPips"
                        :key="index"
                        :label="item"
                        border
                        checked
                        class="mt-3"
                      />
                    </el-checkbox-group>
                  </div>

                  <div
                    v-if="schema.allowCommissions.length > 0"
                    class="row mb-5"
                  >
                    <label class="mt-5">{{
                      $t("fields.availableCommission")
                    }}</label>
                    <el-checkbox-group
                      v-model="schema.allowCommissions"
                      size="large"
                      disabled
                    >
                      <el-checkbox
                        v-for="(item, index) in schema.allowCommissions"
                        :key="index"
                        :label="item"
                        border
                        checked
                        class="mt-3"
                      />
                    </el-checkbox-group>
                  </div>
                </div>
              </div>

              <div
                v-if="salesLinkDetail.serviceType == ReferralServiceType.Client"
                class="row"
                style="position: relative"
              >
                <div
                  class="col-12 mb-7"
                  v-for="(schema, index) in salesLinkDetail.summary
                    .allowAccountTypes"
                  :key="index"
                  style="
                    border-radius: 10px;
                    box-shadow: rgba(0, 0, 0, 0.16) 0px 1px 4px;
                    padding: 10px 4px;
                  "
                >
                  <div
                    class="d-flex align-items-center justify-content-between"
                  >
                    <div class="d-flex align-items-center">
                      <input
                        :id="schema"
                        class="form-check-input widget-9-check me-2"
                        type="checkbox"
                        :name="schema"
                        checked
                        disabled
                      />
                      <div class="d-flex flex-column">
                        <div class="fs-6 text-center">
                          <label for="rebateStdAccount">{{
                            $t("type.account." + schema.accountType)
                          }}</label>
                        </div>
                        <div class="ms-2 me-2">
                          <span class="badge badge-primary">
                            {{
                              schema.optionName == "alpha"
                                ? $t("type.shortAccount.alpha")
                                : schema.optionName
                            }}</span
                          >
                        </div>
                      </div>
                    </div>
                    <div class="d-flex">
                      <div>
                        <span class="badge badge-success"
                          >{{ $t("title.pips") }}
                        </span>
                        <span class="ms-2 me-2 fs-5">=></span>
                        <span class="badge badge-success">{{
                          schema.pips == null || schema.pips == 0
                            ? "0"
                            : schema.pips
                        }}</span>
                      </div>
                      <span class="ms-3 me-3 fs-5">|</span>
                      <div>
                        <span class="badge badge-info"
                          >{{ $t("title.commission") }}
                        </span>
                        <span class="ms-2 me-2 fs-5">=></span>
                        <span class="badge badge-info">{{
                          schema.commission == null || schema.commission == 0
                            ? "0"
                            : schema.commission
                        }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useI18n } from "vue-i18n";
import { Field } from "vee-validate";
import { AccountTypes } from "@/core/types/AccountInfos";
import { showModal, hideModal } from "@/core/helpers/dom";
import { ReferralServiceType } from "@/core/types/ReferralServiceType";

import SalesService from "../services/SalesService";

const { t } = useI18n();
const isLoading = ref(true);
const salesLinkDetail = ref();
const accountType = ref([] as any);
const SalesLinkDetailModalRef = ref<null | HTMLElement>(null);

const show = async (_code: string) => {
  isLoading.value = true;
  accountType.value = [];
  showModal(SalesLinkDetailModalRef.value);

  try {
    salesLinkDetail.value = await SalesService.getSalesLinkDetail(_code);

    salesLinkDetail.value.serviceType =
      salesLinkDetail.value.serviceType.toString();

    if (salesLinkDetail.value.serviceType == ReferralServiceType.Broker) {
      salesLinkDetail.value.summary.schema.forEach((item) => {
        item.allowCommissions = item.allowCommissions.map((acc) =>
          t("type.pipCommission.commission." + acc)
        );
        item.allowPips = item.allowPips.map((acc) =>
          t("type.pipCommission.pips." + acc)
        );
      });
    }

    if (Object.keys(salesLinkDetail.value?.summary).length === 0) {
      salesLinkDetail.value = null;
    }
  } catch (error) {
    salesLinkDetail.value = null;
  } finally {
    isLoading.value = false;
  }
};

const hide = () => {
  hideModal(SalesLinkDetailModalRef.value);
};

defineExpose({
  hide,
  show,
});
</script>

<style scoped lang="scss">
.dot {
  width: 10px;
  height: 10px;

  border-radius: 100px;
  background: #0a46aa;
}
.step {
  font-family: "Lato", sans-serif;
  font-weight: 400;
  font-size: 18px;
  color: #212121;
}

.stepContent {
  font-family: "Lato", sans-serif;
  font-weight: 400;
  font-size: 16px;
  color: #4d4d4d;
}

.IB-rebate-link-select {
  padding: 16px 20px;
  width: 300px;
  height: 56px;
}
</style>

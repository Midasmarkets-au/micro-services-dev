<template>
  <SimpleForm
    ref="salesRebateSchemaDetailShowRef"
    :title="$t('rebate.salesRebateSchema.schemaDetail')"
    :is-loading="isLoading"
    :width="800"
    disable-footer
  >
    <div>
      <div v-if="isLoading" class="d-flex justify-content-center">
        <LoadingRing />
      </div>
      <div v-else>
        <!-- ============================================================================ -->
        <hr class="mt-5 mb-9" />
        <!-- ============================================================================ -->

        <div class="row mb-7">
          <div class="col-6">
            <label class="fs-6 fw-semobold mb-2 createAccountTitle">
              {{ $t("rebate.salesRebateSchema.salesType") }}
            </label>
            <el-select
              v-model="schemaInfo.salesType"
              :placeholder="$t('fields.status')"
            >
              <el-option
                v-for="item in salesTypeOptions"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </div>

          <div class="col-6">
            <label class="fs-6 fw-semobold mb-2 createAccountTitle">
              {{ $t("rebate.salesRebateSchema.schedule") }}
              <span
                v-if="schemaInfo.status == -1"
                class="ms-3"
                style="color: #900000"
                >{{ $t("rebate.salesRebateSchema.updateAfterApprove") }}</span
              >
              <span v-else class="ms-3" style="color: #900000">{{
                $t("rebate.salesRebateSchema.updateUidRecords", {
                  uid: schemaInfo.rebateAccountUid,
                })
              }}</span>
            </label>
            <div class="d-flex align-items-center mt-2">
              <input
                id="daily"
                type="radio"
                value="0"
                v-model="schemaInfo.schedule"
              />
              <label class="ms-1 me-7" for="daily">{{
                $t("rebate.salesRebateSchema.daily")
              }}</label>
              <input
                id="month"
                type="radio"
                value="3"
                v-model="schemaInfo.schedule"
              />
              <label class="ms-1 me-7" for="month">{{
                $t("rebate.salesRebateSchema.monthly")
              }}</label>
              <input
                id="pause"
                type="radio"
                value="1"
                v-model="schemaInfo.schedule"
              />
              <label class="ms-1 me-7" for="pause">{{
                $t("rebate.salesRebateSchema.pause")
              }}</label>
            </div>
          </div>
        </div>

        <div class="row mb-5">
          <div class="col-6">
            <label class="required fs-6 fw-semobold mb-2 createAccountTitle">
              {{ $t("rebate.salesRebateSchema.salesAccountUid") }}
            </label>
            <Field
              v-model="schemaInfo.salesAccountUid"
              class="form-control form-control-lg form-control-solid"
              type="number"
              name="symbolRule"
              autocomplete="off"
            >
              <el-input
                v-model="schemaInfo.salesAccountUid"
                size="large"
                disabled
              >
              </el-input>
            </Field>
          </div>
          <div class="col-6">
            <label class="required fs-6 fw-semobold mb-2 createAccountTitle">
              {{ $t("rebate.salesRebateSchema.targetAccountUid") }}
            </label>
            <Field
              v-model="schemaInfo.rebateAccountUid"
              class="form-control form-control-lg form-control-solid"
              type="number"
              name="symbolRule"
              autocomplete="off"
            >
              <el-input
                v-model="schemaInfo.rebateAccountUid"
                size="large"
                disabled
              >
              </el-input>
            </Field>
          </div>
        </div>

        <div class="row mb-9">
          <div class="col-6">
            <label class="required fs-6 fw-semobold mb-2 createAccountTitle">
              {{ $t("rebate.salesRebateSchema.standard") }}
            </label>
            <Field
              v-model="schemaInfo.rebate"
              class="form-control form-control-lg form-control-solid"
              type="number"
              name="symbolRule"
              autocomplete="off"
            >
              <el-input v-model="schemaInfo.rebate" size="large">
                <template #append>/100</template>
              </el-input>
            </Field>
          </div>
          <div class="col-6">
            <label class="required fs-6 fw-semobold mb-2 createAccountTitle">
              {{ $t("rebate.salesRebateSchema.rawRebate") }}
            </label>
            <Field
              v-model="schemaInfo.alphaRebate"
              class="form-control form-control-lg form-control-solid"
              type="number"
              name="symbolRule"
              autocomplete="off"
            >
              <el-input v-model="schemaInfo.alphaRebate" size="large">
                <template #append>/100</template>
              </el-input>
            </Field>
          </div>
        </div>

        <div class="row mb-9">
          <div class="col-6">
            <label class="required fs-6 fw-semobold mb-2 createAccountTitle">
              {{ $t("rebate.salesRebateSchema.excludeAccount") }}
            </label>
            <Field
              v-model="schemaInfo.excludeAccount"
              class="form-control form-control-lg form-control-solid"
              name="symbolRule"
              autocomplete="off"
            >
              <el-input
                v-model="schemaInfo.excludeAccount"
                size="large"
                :rows="5"
                type="textarea"
              >
              </el-input>
            </Field>
          </div>
          <div class="col-6">
            <label class="required fs-6 fw-semobold mb-2 createAccountTitle">
              {{ $t("rebate.salesRebateSchema.excludeSymbol") }}
            </label>
            <Field
              v-model="schemaInfo.excludeSymbol"
              class="form-control form-control-lg form-control-solid"
              name="symbolRule"
              autocomplete="off"
            >
              <el-input
                v-model="schemaInfo.excludeSymbol"
                size="large"
                type="textarea"
                :rows="5"
              >
              </el-input>
            </Field>
          </div>
        </div>

        <div class="row mb-5">
          <div class="col-12">
            <label class="required fs-6 fw-semobold mb-2 createAccountTitle">
              {{ $t("rebate.salesRebateSchema.note") }}
            </label>
            <Field
              v-model="schemaInfo.note"
              class="form-control form-control-lg form-control-solid"
              name="symbolRule"
              autocomplete="off"
            >
              <el-input
                v-model="schemaInfo.note"
                size="large"
                :rows="5"
                type="textarea"
              >
              </el-input>
            </Field>
          </div>
        </div>

        <div class="row mb-9">
          <div class="col-12 d-flex align-items-center gap-3">
            <label class="fs-6 fw-semobold createAccountTitle">
              Auto Release
            </label>
            <el-switch v-model="schemaInfo.autoRelease" />
            <span class="text-muted fs-7"
              >Automatically credit wallet after settlement</span
            >
          </div>
        </div>

        <div
          v-if="schemaInfo.status != -1"
          style="border: 2px solid #e6a23c; border-radius: 10px; padding: 10px"
        >
          <div class="d-flex align-items-center mb-4" style="font-size: 18px">
            <i
              class="fa-solid fa-triangle-exclamation fa-xl me-3"
              style="color: #900000"
            ></i>
            <div>{{ $t("rebate.salesRebateSchema.reapprovalNote") }}</div>
          </div>
        </div>
        <div class="mt-13 d-flex flex-row-reverse">
          <button class="btn btn-light btn-danger btn-lg me-3 mb-9">
            Cancel
          </button>
          <button
            v-if="schemaInfo.status == -1 && $can('SuperAdmin')"
            class="btn btn-light btn-warning btn-lg me-3 mb-9"
            @click="approve"
          >
            Approve
          </button>
          <button
            class="btn btn-light btn-success btn-lg me-3 mb-9"
            @click="update"
          >
            Update
          </button>
        </div>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref, nextTick } from "vue";
import RebateService from "../services/RebateService";
import SimpleForm from "@/components/SimpleForm.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { Field } from "vee-validate";
import { salesTypeOptions } from "@/core/types/SalesTypes";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const emits = defineEmits<{
  (e: "refresh"): void;
}>();

const isLoading = ref(true);
const salesRebateSchemaDetailShowRef = ref<any>(null);
const schemaInfo = ref({} as any);

const show = async (_item: any) => {
  salesRebateSchemaDetailShowRef.value?.show();

  isLoading.value = true;
  schemaInfo.value = JSON.parse(JSON.stringify(_item));
  schemaInfo.value.excludeAccount = schemaInfo.value.excludeAccount.join(",");
  schemaInfo.value.excludeSymbol = schemaInfo.value.excludeSymbol.join(",");
  isLoading.value = false;
};

const isEmpty = (v: any) => v === "" || v === null || v === undefined;

const update = async () => {
  const { rebate, alphaRebate } = schemaInfo.value;
  if (isEmpty(rebate) || isEmpty(alphaRebate)) {
    MsgPrompt.error(t("rebate.salesRebateSchema.fillRebate"));
    return;
  }
  if (isEmpty(schemaInfo.value.proRebate)) schemaInfo.value.proRebate = "0";
  try {
    await RebateService.putSalesRebateSchema(
      schemaInfo.value.id,
      schemaInfo.value
    );
    emits("refresh");
    salesRebateSchemaDetailShowRef.value?.hide();
  } catch (e) {
    MsgPrompt.error(e);
  }
};

const approve = async () => {
  try {
    await RebateService.putSalesRebateSchemaStatus(schemaInfo.value.id);
    emits("refresh");
    salesRebateSchemaDetailShowRef.value?.hide();
  } catch (e) {
    MsgPrompt.error(e);
  }
};

defineExpose({ show });
</script>

<style>
.rebate-long-btn {
  width: 100%;
  height: 40px;
  font-size: 24px;
  border-radius: 8px;
  color: gray;
  background-color: white;
  border: 1px solid lightgray;
}

.rebate-long-btn:focus {
  background-color: lightgray;
}
</style>

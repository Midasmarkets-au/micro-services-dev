<template>
  <SimpleForm
    ref="addSalesRebateSchemaShowRef"
    :title="'New Schema'"
    :is-loading="false"
    :width="800"
    disable-footer
  >
    <div>
      <div>
        <!-- ============================================================================ -->
        <hr class="mt-5 mb-9" />
        <!-- ============================================================================ -->

        <div class="row mb-7">
          <div class="col-6">
            <label class="fs-6 fw-semobold mb-2 createAccountTitle">
              Sales Type
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
              Schedule
              <span class="ms-3" style="color: #900000"
                >( All records will be updated after approve !! )</span
              >
            </label>
            <div class="d-flex align-items-center mt-2">
              <input
                id="daily"
                type="radio"
                value="0"
                v-model="schemaInfo.schedule"
              />
              <label class="ms-1 me-7" for="daily">Daily</label>
              <input
                id="month"
                type="radio"
                value="3"
                v-model="schemaInfo.schedule"
              />
              <label class="ms-1 me-7" for="month">Monthly</label>
              <input
                id="pause"
                type="radio"
                value="1"
                v-model="schemaInfo.schedule"
              />
              <label class="ms-1 me-7" for="pause">Pause</label>
            </div>
          </div>
        </div>

        <div class="row mb-5">
          <div class="col-6">
            <label class="fs-6 fw-semobold mb-2 createAccountTitle">
              Sales Account Uid
            </label>
            <Field
              v-model="schemaInfo.salesAccountUid"
              class="form-control form-control-lg form-control-solid"
              type="number"
              name="symbolRule"
              autocomplete="off"
            >
              <el-input v-model="schemaInfo.salesAccountUid" size="large">
              </el-input>
            </Field>
          </div>
          <div class="col-6">
            <label class="fs-6 fw-semobold mb-2 createAccountTitle">
              Target Account Uid
            </label>
            <Field
              v-model="schemaInfo.rebateAccountUId"
              class="form-control form-control-lg form-control-solid"
              type="number"
              name="symbolRule"
              autocomplete="off"
            >
              <el-input v-model="schemaInfo.rebateAccountUId" size="large">
              </el-input>
            </Field>
          </div>
        </div>

        <div class="row mb-9">
          <div class="col-4">
            <label class="fs-6 fw-semobold mb-2 createAccountTitle">
              Rebate
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
          <div class="col-4">
            <label class="fs-6 fw-semobold mb-2 createAccountTitle">
              Raw Rebate
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
          <div class="col-4">
            <label class="fs-6 fw-semobold mb-2 createAccountTitle">
              Pro Rebate
            </label>
            <Field
              v-model="schemaInfo.proRebate"
              class="form-control form-control-lg form-control-solid"
              type="number"
              name="symbolRule"
              autocomplete="off"
            >
              <el-input v-model="schemaInfo.proRebate" size="large">
                <template #append>/100</template>
              </el-input>
            </Field>
          </div>
        </div>

        <div class="row mb-9">
          <div class="col-6">
            <label class="fs-6 fw-semobold mb-2 createAccountTitle">
              Exclude Account
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
            <label class="fs-6 fw-semobold mb-2 createAccountTitle">
              Exclude Symbol
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
            <label class="fs-6 fw-semobold mb-2 createAccountTitle">
              Note
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
        <div class="mt-13 d-flex flex-row-reverse">
          <button
            class="btn btn-light btn-danger btn-lg me-3 mb-9"
            @click="reset"
          >
            Cancel
          </button>
          <button
            class="btn btn-light btn-success btn-lg me-3 mb-9"
            @click="update"
          >
            Add New
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

const emits = defineEmits<{
  (e: "refresh"): void;
}>();

const addSalesRebateSchemaShowRef = ref<any>(null);
const schemaInfo = ref({} as any);

const reset = () => {
  schemaInfo.value = {
    salesType: 0,
    salesAccountUid: "",
    rebateAccountUId: "",
    rebate: "",
    alphaRebate: "",
    proRebate: "",
    excludeAccount: "",
    excludeSymbol:
      "#AAPL,#AXP,#BAC,#C,#DIS,#IBM,#INTC,#KO,#MCD,#MSFT,#BA,#QAN.AX,#CSL.AX,#BHP.AX,#6501.T,#6502.T,#7201.T,#7261.T,#8306.T",
    note: "",
  };
};

const show = async () => {
  reset();
  addSalesRebateSchemaShowRef.value?.show();
};

const update = async () => {
  try {
    await RebateService.postSalesRebateSchema(schemaInfo.value);
    emits("refresh");
    addSalesRebateSchemaShowRef.value?.hide();
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

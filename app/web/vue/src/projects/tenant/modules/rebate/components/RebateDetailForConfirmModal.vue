<template>
  <SimpleForm
    ref="modalRef"
    :title="'Confirm Pending Rebate'"
    :is-loading="isLoading"
    :width="1300"
    disable-footer
  >
    <div v-if="isLoading" class="d-flex justify-content-center">
      <LoadingRing />
    </div>
    <div v-else>
      <hr />
      <div class="p-5">
        <div class="fv-row mb-7">
          <ul
            class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
          >
            <li class="nav-item">
              <a
                class="nav-link text-active-primary pb-4"
                :class="{ active: formTab === 'rate' }"
                data-bs-toggle="tab"
                href="#"
                @click="formTab = 'rate'"
                >Rate</a
              >
            </li>

            <li class="nav-item">
              <a
                class="nav-link text-active-primary pb-4"
                :class="{ active: formTab === 'pips' }"
                data-bs-toggle="tab"
                href="#"
                @click="formTab = 'pips'"
                >Pips</a
              >
            </li>

            <li class="nav-item">
              <a
                class="nav-link text-active-primary pb-4"
                :class="{ active: formTab === 'commission' }"
                data-bs-toggle="tab"
                href="#"
                @click="formTab = 'commission'"
                >Commission</a
              >
            </li>
          </ul>
        </div>

        <div class="row mb-9">
          <div class="col-3">
            <label class="required fs-6 fw-semobold mb-2 createAccountTitle">
              New Schema Name
            </label>
            <Field
              v-model="schemaInfo.name"
              class="form-control form-control-lg form-control-solid"
              type="number"
              name="symbolRule"
              autocomplete="off"
            >
              <el-input v-model="schemaInfo.name" size="large"> </el-input>
            </Field>
          </div>
          <div class="col-9">
            <label class="required fs-6 fw-semobold mb-2 createAccountTitle">
              Note
            </label>
            <Field
              v-model="schemaInfo.note"
              class="form-control form-control-lg form-control-solid"
              type="number"
              name="symbolRule"
              autocomplete="off"
            >
              <el-input v-model="schemaInfo.note" size="large"> </el-input>
            </Field>
          </div>
        </div>

        <!-- Table Header -->
        <div
          v-for="(symbols, category) in selectRebateSchema"
          :key="category"
          style="box-shadow: rgba(99, 99, 99, 0.2) 0px 2px 8px 0px"
          class="mb-7 p-7"
        >
          <!-- Table Content -->
          <div class="row">
            <div
              class="col-4 mt-5"
              v-for="(rule, symbol) in symbols"
              :key="symbol"
            >
              <Field
                v-model="selectRebateSchema[category][symbol][formTab]"
                type="number"
                name="symbolRule"
                autocomplete="off"
              >
                <el-input
                  v-model="selectRebateSchema[category][symbol][formTab]"
                  size="large"
                >
                  <template #prepend>
                    <div style="width: 80px">
                      <label>{{ symbol }}</label>
                    </div>
                  </template>
                </el-input>
              </Field>
            </div>
          </div>
        </div>

        <div class="mt-13 d-flex flex-row-reverse">
          <button
            class="btn btn-light btn-danger btn-lg me-3 mb-9"
            @click="hide"
          >
            Cancel
          </button>
          <button
            class="btn btn-light btn-primary btn-lg me-3 mb-9"
            @click="confirmDirectRule()"
          >
            Confirm
          </button>
        </div>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { nextTick, ref } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import SimpleForm from "@/components/SimpleForm.vue";
import RebateService from "../services/RebateService";

const emits = defineEmits<{
  (e: "refresh"): void;
}>();

const modalRef = ref<InstanceType<typeof SimpleForm>>();
const isLoading = ref(true);
const selectRebateSchema = ref<any>({});
const formTab = ref("rate");
const selectedRuleID = ref<any>(-1);
const schemaInfo = ref<any>({
  loading: false,
  id: "",
});

const hide = () => {
  modalRef.value?.hide();
};

const refresh = () => {
  emits("refresh");
};

const confirmDirectRule = async () => {
  try {
    await RebateService.putConfirmDirectRebateRule(selectedRuleID.value);

    MsgPrompt.success("Confirm Success").then(() => {
      refresh();
      hide();
    });
  } catch (error) {
    MsgPrompt.error(error);
  }
};

const setUpForm = async (_selectedRule: any) => {
  isLoading.value = true;
  selectedRuleID.value = _selectedRule.id;
  modalRef.value?.show();

  try {
    const resSymbolList = await RebateService.getSymbolsList();

    selectRebateSchema.value = resSymbolList.reduce((acc, item) => {
      const category = item.category;

      if (!acc[category]) {
        acc[category] = {};
      }

      acc[category][item.code] = {
        symbolCode: item.code,
        rate: 0,
        pips: 0,
        commission: 0,
      };

      return acc;
    }, {});

    const categoryMap = {};
    for (const item of resSymbolList) {
      categoryMap[item.code] = item.category;
    }

    const resSchema = await RebateService.getRebateSchema(
      _selectedRule.rebateRuleId
    );

    console.log("resSchema", resSchema);

    schemaInfo.value.name = resSchema.name;
    schemaInfo.value.note = resSchema.note;

    resSchema.rebateRuleItems.forEach(function (item) {
      selectRebateSchema.value[categoryMap[item.symbolCode]][item.symbolCode] =
        {
          symbolCode: item.symbolCode,
          rate: item.rate,
          pips: item.pips,
          commission: item.commission,
        };
    });
  } catch (error) {
    MsgPrompt.error(error);
  }
  isLoading.value = false;
};

defineExpose({
  setUpForm,
  hide,
});
</script>

<style scoped></style>

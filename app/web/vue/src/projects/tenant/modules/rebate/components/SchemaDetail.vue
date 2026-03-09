<template>
  <SimpleForm
    ref="rebateSchemaDetailShowRef"
    :title="'Schema Detail'"
    :is-loading="isLoading"
    :width="1300"
    disable-footer
  >
    <div class="p-5">
      <div v-if="isLoading" class="d-flex justify-content-center">
        <LoadingRing />
      </div>
      <div v-else>
        <!-- ============================================================================ -->
        <hr class="mt-5 mb-9" />
        <!-- ============================================================================ -->

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

        <div
          v-if="
            selectedSchemaType === RebateSchemaType.Rebate ||
            selectedSchemaType === RebateSchemaType.Base
          "
          class="fv-row mb-7"
        >
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

        <!-- Table Header -->
        <!-- symbols, category, index -->

        <div
          v-for="(symbols, category) in editableRebateSchema"
          :key="category"
          style="box-shadow: rgba(99, 99, 99, 0.2) 0px 2px 8px 0px"
          class="mb-7 p-7"
        >
          <div class="d-flex justify-content-between">
            <h3>{{ category }}</h3>
            <div class="d-flex gap-2">
              <button
                class="btn-toggle"
                :class="{ active: expandedPipOptions.includes(category) }"
                @click="togglePipOption(category)"
              >
                {{
                  expandedPipOptions.includes(category)
                    ? $t("action.collapse")
                    : $t("action.expand")
                }}
              </button>
              <div class="w-200px">
                <Field
                  v-model="applyCategoryForm[formTab][category]"
                  class="form-control form-control-lg form-control-solid"
                  type="text"
                  name="applyCategory"
                  autocomplete="off"
                >
                  <el-input
                    v-model="applyCategoryForm[formTab][category]"
                    :placeholder="$t('tip.pleaseInput')"
                    size="large"
                  >
                    <template #append>
                      <el-button @click.prevent="applyCategoryValue(category)">
                        <span>Apply</span>
                      </el-button>
                    </template>
                  </el-input>
                </Field>
              </div>
            </div>
          </div>

          <!-- Table Content -->
          <div
            class="row"
            style="max-height: 300px; overflow-y: auto"
            v-if="expandedPipOptions.includes(category)"
          >
            <div
              class="col-4 mt-5"
              v-for="(symbol, symbolIndex) in symbols"
              :key="symbolIndex"
            >
              <Field
                v-model="editableRebateSchema[category][symbolIndex][formTab]"
                class="form-control form-control-lg form-control-solid"
                type="number"
                name="symbolRule"
                autocomplete="off"
              >
                <el-input
                  v-model="editableRebateSchema[category][symbolIndex][formTab]"
                  size="large"
                >
                  <template #prepend>
                    <div style="width: 80px">
                      <label>{{ symbol.symbolCode }}</label>
                    </div>
                  </template>
                </el-input>
              </Field>
            </div>
          </div>
        </div>

        <div class="mt-13 d-flex flex-row-reverse">
          <button class="btn btn-light btn-danger btn-lg me-3 mb-9">
            Cancel
          </button>
          <button
            class="btn btn-light btn-success btn-lg me-3 mb-9"
            @click="postSchema"
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
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import { Field } from "vee-validate";
import { useI18n } from "vue-i18n";
import { RebateSchemaType } from "@/core/types/RebateSchemaType";
const expandedPipOptions = ref<any[]>([]);
const emits = defineEmits<{
  (e: "refresh"): void;
}>();

const { t } = useI18n();
const isLoading = ref(true);
const formTab = ref("rate");
const selectedSchemaID = ref<number>(0);
const selectedSchemaType = ref<number>(0);
const editableRebateSchema = ref<any>({});
const originalRebateSchema = ref<any>({});
const rebateSchemaDetailShowRef = ref<any>(null);
const schemaInfo = ref({} as any);
const schemaTable = ref({} as any);

const applyCategoryForm = ref({
  rate: {},
  commission: {},
  pips: {},
  bundleVal: {},
});

const reset = () => {
  applyCategoryForm.value = {
    rate: {},
    commission: {},
    pips: {},
    bundleVal: {},
  };

  formTab.value = "rate";
  schemaInfo.value.schemaName = "";
  schemaInfo.value.schemaNote = "";
};

const applyCategoryValue = (_category: any) => {
  Object.keys(editableRebateSchema.value[_category]).forEach((key) => {
    editableRebateSchema.value[_category][key][formTab.value] =
      applyCategoryForm.value[formTab.value][_category];
  });
};
const togglePipOption = (key: string | number) => {
  const index = expandedPipOptions.value.indexOf(key);
  if (index > -1) {
    expandedPipOptions.value.splice(index, 1);
  } else {
    expandedPipOptions.value.push(key);
  }
};
const show = async (_id: number, _schemaType: number) => {
  rebateSchemaDetailShowRef.value?.show();

  isLoading.value = true;
  selectedSchemaID.value = _id;
  selectedSchemaType.value = _schemaType;

  // Prepare table: setup form and fillup with data
  try {
    const resSymbolList = await RebateService.getSymbolsList();
    originalRebateSchema.value = resSymbolList.reduce((acc, item) => {
      const category = item.category;
      if (!expandedPipOptions.value.includes(category)) {
        expandedPipOptions.value.push(category);
      }
      if (!acc[category]) {
        acc[category] = {};
      }

      if (
        _schemaType == RebateSchemaType.Base ||
        _schemaType == RebateSchemaType.Rebate
      ) {
        acc[category][item.code] = {
          symbolCode: item.code,
          rate: 0,
          pips: 0,
          commission: 0,
        };
      } else {
        formTab.value = "bundleVal";
        acc[category][item.code] = {
          symbolCode: item.code,
          bundleVal: 0,
        };
      }
      return acc;
    }, {});

    editableRebateSchema.value = JSON.parse(
      JSON.stringify(originalRebateSchema.value)
    );

    const categoryMap = {};
    for (const item of resSymbolList) {
      categoryMap[item.code] = item.category;
    }

    if (
      _schemaType == RebateSchemaType.Rebate ||
      _schemaType == RebateSchemaType.Base
    ) {
      if (_schemaType == RebateSchemaType.Base) {
        schemaInfo.value = await RebateService.getRebateBaseSchemaById(_id);
        schemaTable.value = schemaInfo.value.rebateBaseSchemaItems;
      } else if (_schemaType == RebateSchemaType.Rebate) {
        schemaInfo.value = await RebateService.getRebateSchema(_id);
        schemaTable.value = schemaInfo.value.rebateDirectSchemaItems;
      }

      schemaTable.value.forEach(function (item: {
        symbolCode: string | number;
        rate: any;
        pips: any;
        commission: any;
      }) {
        if (
          categoryMap[item.symbolCode] !== undefined &&
          editableRebateSchema.value[categoryMap[item.symbolCode]][
            item.symbolCode
          ] !== undefined
        ) {
          editableRebateSchema.value[categoryMap[item.symbolCode]][
            item.symbolCode
          ] = {
            symbolCode: item.symbolCode,
            rate: item.rate,
            pips: item.pips,
            commission: item.commission,
          };
        }
      });
    } else {
      schemaInfo.value = await RebateService.getRebateSchemaBundleById(_id);
      const symbols = Object.keys(schemaInfo.value.items);
      for (const symbol of symbols) {
        if (
          categoryMap[symbol] !== undefined &&
          editableRebateSchema.value[categoryMap[symbol]][symbol] !== undefined
        ) {
          editableRebateSchema.value[categoryMap[symbol]][symbol] = {
            symbolCode: symbol,
            bundleVal: schemaInfo.value.items[symbol],
          };
        }
      }
    }
    isLoading.value = false;
  } catch (error) {
    console.log(error);
  }
};

const postSchema = async () => {
  const data = {
    RebateRuleItems: Object.keys(editableRebateSchema.value).reduce(
      (result: any, category) => {
        const symbols = editableRebateSchema.value[category];
        Object.keys(symbols).forEach((symbol) => {
          // If the schema type is Base || Rebate
          if (
            selectedSchemaType.value == RebateSchemaType.Base ||
            selectedSchemaType.value == RebateSchemaType.Rebate
          ) {
            const { symbolCode, rate, pips, commission } = symbols[symbol];
            result.push({ symbolCode, rate, pips, commission });
          } else {
            result[symbols[symbol].symbolCode] = symbols[symbol].bundleVal;
          }
        });
        return result;
      },
      selectedSchemaType.value == RebateSchemaType.Base ||
        selectedSchemaType.value == RebateSchemaType.Rebate
        ? []
        : {}
    ),
  };

  try {
    if (selectedSchemaType.value == RebateSchemaType.Rebate) {
      await RebateService.putRebateSchema(selectedSchemaID.value, {
        name: schemaInfo.value.name,
        note: schemaInfo.value.note,
      });
      await RebateService.putRebateSchemaItems(
        selectedSchemaID.value,
        data.RebateRuleItems
      );
    } else if (selectedSchemaType.value == RebateSchemaType.Base) {
      await RebateService.putRebateBaseSchema(selectedSchemaID.value, {
        name: schemaInfo.value.name,
        note: schemaInfo.value.note,
      });
      await RebateService.putRebateBaseSchemaItems(
        selectedSchemaID.value,
        data.RebateRuleItems
      );
    } else {
      await RebateService.putRebateSchemaBundle(selectedSchemaID.value, {
        name: schemaInfo.value.name,
        note: schemaInfo.value.note,
        type: selectedSchemaType.value,
        items: data.RebateRuleItems,
      });
    }

    MsgPrompt.success(t("tip.submitSuccess")).then(() => {
      emits("refresh");
      hide();
      reset();
    });
  } catch (error) {
    MsgPrompt.error(error);
  }
};

const hide = () => {
  rebateSchemaDetailShowRef.value?.hide();
};

defineExpose({ show });
</script>

<style lang="scss">
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
.btn-toggle {
  padding: 4px 8px;
  border: 1px solid #d1d5db;
  background: white;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
  color: #374151;
  transition: all 0.2s;

  &:hover {
    border-color: #3b82f6;
  }

  &.active {
    background: #3b82f6;
    color: white;
    border-color: #3b82f6;
  }
}
</style>

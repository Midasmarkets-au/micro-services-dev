<template>
  <SimpleForm
    ref="modalRef"
    :title="'New Rebate Schema / Template'"
    :is-loading="isLoading"
    :width="1300"
    disable-footer
  >
    <div v-if="isLoading" class="d-flex justify-content-center">
      <LoadingRing />
    </div>
    <div class="p-5" v-else>
      <div class="row">
        <!-- <div v-if="schemaType === RebateSchemaType.Base" class="col-3 mb-5"> -->
        <div
          v-if="
            schemaType === RebateSchemaType.Base ||
            schemaType === RebateSchemaType.Rebate
          "
          class="col-3 mb-5"
        >
          <label class="form-label fs-6 fw-bold text-dark"
            >Prefill Base Schema</label
          >
          <el-select
            v-model="schemaInfo[RebateSchemaType.Base].id"
            filterable
            clearable
            remote
            reserve-keyword
            placeholder="Please enter a keyword"
            :remote-method="remoteMethod"
            :loading="schemaInfo[RebateSchemaType.Base]['loading']"
            @click="currentDropdown = RebateSchemaType.Base"
            @change="preFillTemplate(RebateSchemaType.Base)"
          >
            <el-option
              v-for="item in schemaInfo[RebateSchemaType.Base].options"
              :key="item.value"
              :label="item.key"
              :value="item.value"
            />
          </el-select>
        </div>

        <div v-if="schemaType === RebateSchemaType.Rebate" class="col-3 mb-5">
          <label class="form-label fs-6 fw-bold text-dark"
            >Prefill Rebate Schema</label
          >
          <el-select
            v-model="schemaInfo[RebateSchemaType.Rebate].id"
            clearable
            filterable
            remote
            reserve-keyword
            placeholder="Please enter a keyword"
            :remote-method="remoteMethod"
            :loading="schemaInfo[RebateSchemaType.Rebate]['loading']"
            @click="currentDropdown = RebateSchemaType.Rebate"
            @change="preFillTemplate(RebateSchemaType.Rebate)"
          >
            <el-option
              v-for="item in schemaInfo[RebateSchemaType.Rebate].options"
              :key="item.value"
              :label="item.key"
              :value="item.value"
            />
          </el-select>
        </div>

        <div
          v-if="
            schemaType === RebateSchemaType.Base ||
            schemaType === RebateSchemaType.Rebate ||
            schemaType === RebateSchemaType.Rate
          "
          class="col-3 mb-5"
        >
          <label class="form-label fs-6 fw-bold text-dark">Prefill Rate</label>
          <el-select
            v-model="schemaInfo[RebateSchemaType.Rate].id"
            clearable
            filterable
            remote
            reserve-keyword
            placeholder="Please enter a keyword"
            :remote-method="remoteMethod"
            :loading="schemaInfo[RebateSchemaType.Rate]['loading']"
            @click="currentDropdown = RebateSchemaType.Rate"
            @change="preFillBundle(RebateSchemaType.Rate)"
          >
            <el-option
              v-for="item in schemaInfo[RebateSchemaType.Rate].options"
              :key="item.value"
              :label="item.key"
              :value="item.value"
            />
          </el-select>
        </div>
        <div
          v-if="
            schemaType === RebateSchemaType.Base ||
            schemaType === RebateSchemaType.Rebate ||
            schemaType === RebateSchemaType.Pips
          "
          class="col-3 mb-5"
        >
          <label class="form-label fs-6 fw-bold text-dark">Prefill Pips</label>
          <el-select
            v-model="schemaInfo[RebateSchemaType.Pips].id"
            clearable
            filterable
            remote
            reserve-keyword
            placeholder="Please enter a keyword"
            :remote-method="remoteMethod"
            :loading="schemaInfo[RebateSchemaType.Pips]['loading']"
            @click="currentDropdown = RebateSchemaType.Pips"
            @change="preFillBundle(RebateSchemaType.Pips)"
          >
            <el-option
              v-for="item in schemaInfo[RebateSchemaType.Pips].options"
              :key="item.value"
              :label="item.key"
              :value="item.value"
            />
          </el-select>
        </div>
        <div
          v-if="
            schemaType === RebateSchemaType.Base ||
            schemaType === RebateSchemaType.Rebate ||
            schemaType === RebateSchemaType.Commission
          "
          class="col-3 mb-5"
        >
          <label class="form-label fs-6 fw-bold text-dark"
            >Prefill Commission</label
          >
          <el-select
            v-model="schemaInfo[RebateSchemaType.Commission].id"
            clearable
            filterable
            remote
            reserve-keyword
            placeholder="Please enter a keyword"
            :remote-method="remoteMethod"
            :loading="schemaInfo[RebateSchemaType.Commission]['loading']"
            @click="currentDropdown = RebateSchemaType.Commission"
            @change="preFillBundle(RebateSchemaType.Commission)"
          >
            <el-option
              v-for="item in schemaInfo[RebateSchemaType.Commission].options"
              :key="item.value"
              :label="item.key"
              :value="item.value"
            />
          </el-select>
        </div>
      </div>

      <div class="mt-11">
        <div
          v-if="
            schemaType === RebateSchemaType.Base ||
            schemaType === RebateSchemaType.Rebate
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

        <hr class="mt-5 mb-5" />

        <div class="row mb-9">
          <div class="col-3">
            <label class="required fs-6 fw-semobold mb-2 createAccountTitle">
              New Schema Name
            </label>
            <Field
              v-model="schemaInfo.schemaName"
              class="form-control form-control-lg form-control-solid"
              type="number"
              name="symbolRule"
              autocomplete="off"
            >
              <el-input v-model="schemaInfo.schemaName" size="large">
              </el-input>
            </Field>
          </div>
          <div class="col-9">
            <label class="required fs-6 fw-semobold mb-2 createAccountTitle">
              Note
            </label>
            <Field
              v-model="schemaInfo.schemaNote"
              class="form-control form-control-lg form-control-solid"
              type="number"
              name="symbolRule"
              autocomplete="off"
            >
              <el-input v-model="schemaInfo.schemaNote" size="large">
              </el-input>
            </Field>
          </div>
        </div>
        <!-- 产品列表 -->
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
            @click="postSchema()"
          >
            Add New Schema
          </button>
        </div>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref, nextTick } from "vue";
import { computedAsync } from "@vueuse/core";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import SimpleForm from "@/components/SimpleForm.vue";
import { Field, ErrorMessage, useForm } from "vee-validate";
import { useI18n } from "vue-i18n";
import RebateService from "../services/RebateService";
import { RebateSchemaType } from "@/core/types/RebateSchemaType";
const expandedPipOptions = ref<any[]>([]);
const emits = defineEmits<{
  (e: "refresh"): void;
}>();

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

const modalRef = ref<InstanceType<typeof SimpleForm>>();

const isLoading = ref(true);

const schemaInfo = ref({
  schemaName: "",
  schemaNote: "",
});

const editableRebateSchema = ref<any>({});
const originalRebateSchema = ref<any>({});
const formTab = ref("rate");
const { t } = useI18n();
const schemaType = ref(0);
const categoryMap = ref({});
const currentDropdown = ref(0);

const togglePipOption = (key: string | number) => {
  const index = expandedPipOptions.value.indexOf(key);
  if (index > -1) {
    expandedPipOptions.value.splice(index, 1);
  } else {
    expandedPipOptions.value.push(key);
  }
};
const show = async (_schemaType: number) => {
  modalRef.value?.show();
  isLoading.value = true;
  schemaType.value = _schemaType;

  await setupDropdownOptions();

  try {
    const res = await RebateService.getSymbolsList();
    originalRebateSchema.value = res.reduce((acc, item) => {
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
    for (const item of res) {
      categoryMap.value[item.code] = item.category;
    }
  } catch (error) {
    MsgPrompt.error(error);
  }

  isLoading.value = false;
};

const remoteMethod = async (query: string) => {
  if (query) {
    schemaInfo.value[currentDropdown.value]["loading"] = true;

    const _handler = {
      [RebateSchemaType.Rebate]: () =>
        RebateService.getRebateSchemaList({
          keyword: query,
        }),
      [RebateSchemaType.Base]: () =>
        RebateService.getBaseSchemaList({ keyword: query }),
      [RebateSchemaType.Rate]: () =>
        RebateService.getBundleList({
          type: RebateSchemaType.Rate,
          keyword: query,
        }),
      [RebateSchemaType.Pips]: () =>
        RebateService.getBundleList({
          type: RebateSchemaType.Pips,
          keyword: query,
        }),
      [RebateSchemaType.Commission]: () =>
        RebateService.getBundleList({
          type: RebateSchemaType.Commission,
          keyword: query,
        }),
    }[currentDropdown.value];
    if (!_handler) {
      MsgPrompt.error(t("tip.fail"));
      return;
    }

    const options = await _handler();
    schemaInfo.value[currentDropdown.value]["options"] = options.data;
    schemaInfo.value[currentDropdown.value]["loading"] = false;
  } else {
    schemaInfo.value[currentDropdown.value]["options"] = [];
  }
};

const setupDropdownOptions = async () => {
  schemaInfo.value[RebateSchemaType.Rebate] = {};
  schemaInfo.value[RebateSchemaType.Base] = {};
  schemaInfo.value[RebateSchemaType.Rate] = {};
  schemaInfo.value[RebateSchemaType.Pips] = {};
  schemaInfo.value[RebateSchemaType.Commission] = {};

  // const resRebate = await RebateService.getRebateSchemaList();
  // const resBase = await RebateService.getBaseSchemaList();
  // const resRate = await RebateService.getBundleList({
  //   type: RebateSchemaType.Rate,
  // });
  // const resPips = await RebateService.getBundleList({
  //   type: RebateSchemaType.Pips,
  // });

  // const resCommission = await RebateService.getBundleList({
  //   type: RebateSchemaType.Commission,
  // });

  // schemaInfo.value[RebateSchemaType.Rebate]["list"] = resRebate.data;
  // schemaInfo.value[RebateSchemaType.Base]["list"] = resBase.data;
  // schemaInfo.value[RebateSchemaType.Rate]["list"] = resRate.data;
  // schemaInfo.value[RebateSchemaType.Pips]["list"] = resPips.data;
  // schemaInfo.value[RebateSchemaType.Commission]["list"] = resCommission.data;

  schemaInfo.value[RebateSchemaType.Rebate]["loading"] = false;
  schemaInfo.value[RebateSchemaType.Base]["loading"] = false;
  schemaInfo.value[RebateSchemaType.Rate]["loading"] = false;
  schemaInfo.value[RebateSchemaType.Pips]["loading"] = false;
  schemaInfo.value[RebateSchemaType.Commission]["loading"] = false;
};

const applyCategoryValue = (_category: any) => {
  Object.keys(editableRebateSchema.value[_category]).forEach((key) => {
    editableRebateSchema.value[_category][key][formTab.value] =
      applyCategoryForm.value[formTab.value][_category];
  });
};

const preFillTemplate = async (_preFillSchemaType) => {
  if (schemaInfo.value[_preFillSchemaType].id.length == 0) {
    refillschema();
    return;
  }

  if (schemaType.value == RebateSchemaType.Base) {
    const resBase = await RebateService.getRebateBaseSchemaById(
      parseInt(schemaInfo.value[_preFillSchemaType].id)
    );
    schemaInfo.value[_preFillSchemaType].table = resBase.rebateBaseSchemaItems;
  } else if (schemaType.value == RebateSchemaType.Rebate) {
    const resRate = await RebateService.getRebateBaseSchemaById(
      parseInt(schemaInfo.value[_preFillSchemaType].id)
    );
    schemaInfo.value[_preFillSchemaType].table = resRate.rebateBaseSchemaItems;
  }

  schemaInfo.value[_preFillSchemaType].table.forEach(function (item) {
    editableRebateSchema.value[categoryMap.value[item.symbolCode]][
      item.symbolCode
    ] = {
      symbolCode: item.symbolCode,
      rate: item.rate,
      pips: item.pips,
      commission: item.commission,
    };
  });
};

const preFillBundle = async (_preFillSchemaType: number) => {
  if (schemaInfo.value[_preFillSchemaType].id.length == 0) {
    refillschema();
    return;
  }

  schemaInfo.value[_preFillSchemaType].table =
    await RebateService.getRebateSchemaBundleById(
      parseInt(schemaInfo.value[_preFillSchemaType].id)
    );

  const symbols = Object.keys(schemaInfo.value[_preFillSchemaType].table.items);

  for (const symbol of symbols) {
    const category = categoryMap.value[symbol];
    if (!category || !editableRebateSchema.value[category]) {
      console.warn(
        `Symbol ${symbol} not found in current symbol list, skipping...`
      );
      continue;
    }
    if (
      schemaType.value == RebateSchemaType.Rebate ||
      schemaType.value == RebateSchemaType.Base
    ) {
      switch (_preFillSchemaType) {
        case RebateSchemaType.Rate:
          editableRebateSchema.value[categoryMap.value[symbol]][symbol][
            "rate"
          ] = schemaInfo.value[_preFillSchemaType].table.items[symbol];
          break;
        case RebateSchemaType.Pips:
          editableRebateSchema.value[categoryMap.value[symbol]][symbol][
            "pips"
          ] = schemaInfo.value[_preFillSchemaType].table.items[symbol];
          break;
        case RebateSchemaType.Commission:
          editableRebateSchema.value[categoryMap.value[symbol]][symbol][
            "commission"
          ] = schemaInfo.value[_preFillSchemaType].table.items[symbol];
          break;
      }
    } else {
      editableRebateSchema.value[categoryMap.value[symbol]][symbol] = {
        symbolCode: symbol,
        bundleVal: schemaInfo.value[_preFillSchemaType].table.items[symbol],
      };
    }
  }
};

const refillschema = async () => {
  editableRebateSchema.value = JSON.parse(
    JSON.stringify(originalRebateSchema.value)
  );

  if (
    schemaType.value == RebateSchemaType.Base &&
    schemaInfo.value[RebateSchemaType.Base].id.length != 0
  ) {
    await preFillTemplate(RebateSchemaType.Base);
  } else if (
    schemaType.value == RebateSchemaType.Rebate &&
    schemaInfo.value[RebateSchemaType.Rebate].id.length != 0
  ) {
    await preFillTemplate(RebateSchemaType.Rebate);
  }

  if (schemaInfo.value[RebateSchemaType.Rate].id.length != 0) {
    await preFillBundle(RebateSchemaType.Rate);
  }

  if (schemaInfo.value[RebateSchemaType.Pips].id.length != 0) {
    await preFillBundle(RebateSchemaType.Pips);
  }

  if (schemaInfo.value[RebateSchemaType.Commission].id.length != 0) {
    await preFillBundle(RebateSchemaType.Commission);
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
            schemaType.value == RebateSchemaType.Base ||
            schemaType.value == RebateSchemaType.Rebate
          ) {
            const { symbolCode, rate, pips, commission } = symbols[symbol];
            result.push({ symbolCode, rate, pips, commission });
          } else {
            result[symbols[symbol].symbolCode] = symbols[symbol].bundleVal;
          }
        });
        return result;
      },
      schemaType.value == RebateSchemaType.Base ||
        schemaType.value == RebateSchemaType.Rebate
        ? []
        : {}
    ),
  };

  try {
    if (schemaType.value == RebateSchemaType.Rebate) {
      await RebateService.postRebateSchema({
        name: schemaInfo.value.schemaName,
        note: schemaInfo.value.schemaNote,
        items: data.RebateRuleItems,
      });
    } else if (schemaType.value == RebateSchemaType.Base) {
      await RebateService.postRebateBaseSchema({
        name: schemaInfo.value.schemaName,
        note: schemaInfo.value.schemaNote,
        items: data.RebateRuleItems,
      });
    } else {
      await RebateService.postRebateSchemaBundle({
        name: schemaInfo.value.schemaName,
        note: schemaInfo.value.schemaNote,
        type: schemaType.value,
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

// const refresh = () => {
//   emits("refresh");
// };

const hide = () => {
  modalRef.value?.hide();
};

defineExpose({
  show,
  hide,
});
</script>

<style scoped lang="scss">
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

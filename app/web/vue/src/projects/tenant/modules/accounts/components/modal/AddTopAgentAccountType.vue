<template>
  <div
    class="modal fade"
    id="kt_modal_create_deposit"
    tabindex="-1"
    aria-hidden="true"
    ref="addTopAgentAccountTypeRef"
  >
    <div
      style="
        position: relative;
        background-color: black;
        opacity: 0.5;
        width: 100%;
        height: 100%;
      "
    ></div>
    <div
      class="modal-dialog modal-dialog-centered mw-900px"
      style="position: absolute; top: 0; bottom: 0; right: 0; left: 0"
    >
      <div class="modal-content">
        <div
          class="form fv-plugins-bootstrap5 fv-plugins-framework"
          style="max-height: 90vh; overflow: scroll"
        >
          <!------------------------------------------------------------------- Modal Header -->
          <div class="modal-header">
            <h2 class="fw-bold">Add Account Type</h2>
            <div
              class="btn btn-icon btn-sm btn-active-icon-primary"
              data-bs-dismiss="modal"
            >
              <span class="svg-icon svg-icon-1">
                <inline-svg src="/images/icons/arrows/arr061.svg" />
              </span>
            </div>
          </div>

          <table
            v-if="isLoading"
            class="table table-row-dashed table-row-gray-200 align-middle"
          >
            <tbody>
              <LoadingRing />
            </tbody>
          </table>
          <div v-else class="p-7">
            <div class="d-flex justify-content-end">
              <button
                v-for="(option, index) in defaultLevelSetting"
                :key="index"
                class="btn btn-sm btn-light-success border-0 me-3"
                :class="selectedOption == index ? 'active' : ''"
                @click="selectedOption = index"
              >
                {{ option.optionName }}
              </button>
            </div>

            <hr class="mt-5 mb-5" />

            <div class="row">
              <div
                class="col-2 mb-1"
                v-for="(item, index) in Object.keys(
                  defaultLevelSetting[selectedOption].category
                )"
                :key="index"
              >
                <div>
                  {{ $t("type.clientSymbolCategory." + item) }}
                </div>
                <el-input
                  class="w-100% h-35px mt-1 mb-1"
                  v-model="defaultLevelSetting[selectedOption].category[item]"
                  disabled
                />
              </div>
            </div>

            <div
              v-if="
                defaultLevelSetting[selectedOption].allowPipOptions.length != 0
              "
              class="row mt-7 mb-3"
            >
              <label>{{ t("fields.availablePips") }}</label>
              <el-checkbox-group v-model="allowPips" size="large">
                <el-checkbox
                  v-for="(p, index) in defaultLevelSetting[selectedOption]
                    .allowPipOptions"
                  class="mt-3"
                  :key="'p_' + index"
                  :label="p"
                  border
                  >{{ t("type.pipOptions." + p) }}
                </el-checkbox>
              </el-checkbox-group>
            </div>
            <div
              v-if="
                defaultLevelSetting[selectedOption].allowCommissionOptions
                  .length != 0
              "
              class="row mb-5"
            >
              <label class="mt-5">{{ t("fields.availableCommission") }}</label>
              <el-checkbox-group v-model="allowCommissions" size="large">
                <el-checkbox
                  v-for="(c, index) in defaultLevelSetting[selectedOption]
                    .allowCommissionOptions"
                  class="mt-3"
                  :key="'c_' + index"
                  :label="c"
                  border
                  >{{ t("type.commissionOptions." + c) }}</el-checkbox
                >
              </el-checkbox-group>
            </div>
          </div>
        </div>

        <div class="d-flex justify-content-center">
          <button
            class="btn btn-md btn-light-primary border-0 mb-7"
            style="width: 200px"
            @click="addTopAgentAccountType(uid)"
          >
            Add Account Type
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useI18n } from "vue-i18n";
import { showModal, hideModal } from "@/core/helpers/dom";
import RebateService from "../../../rebate/services/RebateService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import ResetPassword from "@/projects/client/views/auth/ResetPassword.vue";

const emit = defineEmits(["buildRelationForm"]);

const { t } = useI18n();
const isLoading = ref(true);
const productCategory = ref<any>({} as any);
const defaultLevelSetting = ref<any>({} as any);
const addTopAgentAccountTypeRef = ref<null | HTMLElement>(null);
const selectedOption = ref(0);
const ruleId = ref();
const accountType = ref();
const allowCommissions = ref([]);
const allowPips = ref([]);

const addTopAgentAccountType = async () => {
  const selectedRebateSetting = defaultLevelSetting.value[selectedOption.value];

  try {
    await RebateService.updateTopAgentRule(ruleId.value, {
      optionName: selectedRebateSetting.optionName,
      accountType: accountType.value,
      items: productCategory.value.map((category) => ({
        cid: category.key,
        r: selectedRebateSetting.category[category.key] ?? 0,
      })),
      allowPips: allowPips.value,
      allowCommissions: allowCommissions.value,
      pips: null,
      commission: null,
      percentage: 100,
    });
    MsgPrompt.success("Update Success").then(() => {
      emit("buildRelationForm");
      hideModal(addTopAgentAccountTypeRef.value);
    });
  } catch (error) {
    console.log(error);
  }
};

const reset = () => {
  isLoading.value = true;
  selectedOption.value = 0;
  allowCommissions.value = [];
  allowPips.value = [];
};

const show = async (
  _ruleId,
  _accountType,
  _productCategory,
  _defaultLevelSetting
) => {
  reset();
  showModal(addTopAgentAccountTypeRef.value);

  ruleId.value = _ruleId;
  accountType.value = _accountType;
  productCategory.value = _productCategory;
  defaultLevelSetting.value = _defaultLevelSetting[_accountType];

  isLoading.value = false;
};

defineExpose({
  show,
});
</script>

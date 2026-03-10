<template>
  <div
    class="modal fade"
    id="kt_modal_iblibk_detail"
    tabindex="-1"
    aria-hidden="true"
    ref="IBLinkDetailModalRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-750px">
      <div class="modal-content">
        <div class="modal-header" id="kt_modal_new_address_header">
          <h2 class="fs-2">{{ $t("title.ibLinkDetail") }}</h2>

          <div data-bs-dismiss="modal">
            <span class="svg-icon svg-icon-1">
              <inline-svg src="/images/icons/arrows/arr061.svg" />
            </span>
          </div>
        </div>
        <!-- ------------------------------------------------------------------ -->

        <div class="form-outer">
          <table
            v-if="isLoading"
            class="table table-row-dashed table-row-gray-200 align-middle"
          >
            <tbody>
              <LoadingRing />
            </tbody>
          </table>

          <template v-else>
            <div
              class="mt-5 mb-7"
              style="
                box-sizing: border-box;
                padding: 5px 15px;
                border-radius: 100px;
                background-color: #ffecec;
                color: #9f005b;
              "
            >
              <div>
                <span>{{ $t("tip.existRule") }}: </span>
                <span class="me-3">[A] {{ $t("tip.editRule_1") }}</span>
                <span class="me-3">[B] {{ $t("tip.editRule_2") }}</span>
                <span class="me-3">[C] {{ $t("tip.editRule_3") }}</span>
                <span class="me-3">[D] {{ $t("tip.editRule_4") }}</span>
              </div>

              <div>
                <span>{{ $t("tip.newRule") }}: </span>
                <span class="me-3">{{ $t("tip.editRule_5") }}</span>
              </div>
              <!-- <span>{{ $t("tip.editRule_5") }}</span> -->
            </div>
            <div class="mt-5">
              <div class="mt-5 d-flex">
                <div
                  v-for="(account, index) in currentAccountLevelSetting"
                  :key="index"
                >
                  <input
                    class="form-check-input widget-9-check me-3"
                    type="checkbox"
                    :name="'rebateAccount' + account.accountType"
                    v-model="account.selected"
                    :disabled="account.defaultSelected"
                  />
                  <label class="me-9" for="rebateStdAccount">{{
                    $t("type.account." + account.accountType)
                  }}</label>
                </div>
              </div>
            </div>

            <!--------------------------------------------- Rebate table -->

            <div
              v-for="(account, index) in currentAccountLevelSetting"
              :key="index"
            >
              <div v-if="account['selected']">
                <div
                  class="d-flex align-items-center mt-9 mb-3"
                  style="cursor: pointer"
                >
                  <div
                    class="vertical-line"
                    style="
                      border-left: 3px solid #800020;
                      height: 16px;
                      margin-right: 15px;
                    "
                  ></div>
                  <div class="fw-500 fs-4">
                    {{ $t("type.account." + account.accountType) }}
                  </div>
                </div>

                <BaseRebateEditForm
                  ref="BaseRebateFormRef"
                  :isRoot="props.isRoot"
                  :productCategory="productCategory"
                  :targetAccountRebateRule="
                    targetRebateRuleDetail.schema[account.accountType]
                  "
                  :currentAccountRebateRule="account"
                />
              </div>
            </div>
          </template>
        </div>

        <div class="d-flex justify-content-center">
          <button
            class="btn btn-primary btn-md mb-10 mt-5 w-250px"
            @click="updateRebateRule"
          >
            {{ $t("action.updateRebateRule") }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";
import { useStore } from "@/store";
import IbService from "../services/IbService";
import { showModal, hideModal } from "@/core/helpers/dom";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import BaseRebateEditForm from "./form/BaseRebateEditForm.vue";

const props = defineProps<{
  isRoot: boolean;
  productCategory: any;
  currentAccountLevelSetting: any;
}>();

const store = useStore();
const isLoading = ref(true);
const IBLinkDetailModalRef = ref<null | HTMLElement>(null);
const targetRebateRuleDetail = ref();
const ibAccount = computed(() => store.state.AgentModule.agentAccount);
const BaseRebateFormRef = ref<InstanceType<typeof BaseRebateEditForm>>();
const currentAccountLevelSetting = ref({});

const updateRebateRule = async () => {
  const allowAccountRequest = ref([] as any);
  BaseRebateFormRef.value?.forEach(function (formRef) {
    if (formRef.formCheck()) {
      allowAccountRequest.value.push(formRef.collectData());
    }
  });
  console.log(allowAccountRequest.value);

  try {
    await IbService.putIBRebateRule(
      targetRebateRuleDetail.value.id,
      allowAccountRequest.value
    );
    MsgPrompt.success("Update Success").then(() => {
      hide();
    });
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const show = async (_uid: string) => {
  isLoading.value = true;
  showModal(IBLinkDetailModalRef.value);
  currentAccountLevelSetting.value = JSON.parse(
    JSON.stringify(props.currentAccountLevelSetting)
  );

  try {
    const temp = {};
    targetRebateRuleDetail.value = await IbService.getRebateRuleDetailByUid(
      _uid
    );
    targetRebateRuleDetail.value.schema.forEach((account: any) => {
      temp[account.accountType] = account;
    });
    targetRebateRuleDetail.value.schema = temp;

    Object.keys(currentAccountLevelSetting.value).forEach((accType) => {
      if (targetRebateRuleDetail.value.schema[accType]) {
        currentAccountLevelSetting.value[accType]["selected"] = true;
        currentAccountLevelSetting.value[accType]["defaultSelected"] = true;
      } else {
        currentAccountLevelSetting.value[accType]["selected"] = false;
        currentAccountLevelSetting.value[accType]["defaultSelected"] = false;
      }
    });
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};

const hide = () => {
  hideModal(IBLinkDetailModalRef.value);
};

defineExpose({
  hide,
  show,
});
</script>

<template>
  <div
    class="modal fade rounded-3"
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
        <div v-if="!isLoading" class="form-outer">
          <div
            class="mt-5 mb-7"
            style="
              box-sizing: border-box;
              padding: 5px 15px;
              border-radius: 10px;
              background-color: #ffecec;
              color: #9f005b;
            "
          >
            <div>
              <span>{{ $t("tip.existRule") }}: </span>
              <span class="me-3">[A] {{ $t("tip.editRule_1") }}</span>
              <span class="me-3">[B] {{ $t("tip.editRule_2") }}</span>
              <!-- <span class="me-3">[C] {{ $t("tip.editRule_3") }}</span>
              <span class="me-3">[D] {{ $t("tip.editRule_4") }}</span> -->
            </div>

            <div>
              <span>{{ $t("tip.newRule") }}: </span>
              <span class="me-3">{{ $t("tip.editRule_5") }}</span>
            </div>
            <!-- <span>{{ $t("tip.editRule_5") }}</span> -->
          </div>

          <SalesEditTopAgentForm
            v-if="parentRole == AccountRoleTypes.Sales"
            ref="SalesEditTopAgentFormRef"
            :key="SalesEditTopAgentFormComponentKey"
            :productCategory="props.productCategory"
            :parentUid="parentUid"
            :editUid="editUid"
            @hide="hide"
          ></SalesEditTopAgentForm>

          <EditAgentRuleForm
            v-else
            ref="EditAgentRuleFormRef"
            :key="EditAgentRuleFormComponentKey"
            :productCategory="props.productCategory"
            :parentUid="parentUid"
            :editUid="editUid"
            :service="service"
            @hide="hide"
          ></EditAgentRuleForm>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { showModal, hideModal } from "@/core/helpers/dom";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import EditAgentRuleForm from "../forms/EditAgentRuleForm.vue";
import SalesEditTopAgentForm from "@/projects/client/modules/sales/components/form/SalesEditTopAgentForm.vue";

const service = ref(0);
const editUid = ref(0);
const parentUid = ref(0);
const parentRole = ref(0);
const isLoading = ref(true);
const EditAgentRuleFormComponentKey = ref(0);
const SalesEditTopAgentFormComponentKey = ref(0);
const IBLinkDetailModalRef = ref<null | HTMLElement>(null);
const EditAgentRuleFormRef = ref<InstanceType<typeof EditAgentRuleForm>>();
const SalesEditTopAgentFormRef =
  ref<InstanceType<typeof SalesEditTopAgentForm>>();

const props = defineProps<{
  productCategory: any;
}>();

const show = async (
  _service: any,
  _parentRole: any,
  _parentUid: number,
  _editUid: number
) => {
  isLoading.value = true;
  showModal(IBLinkDetailModalRef.value);
  service.value = _service;
  editUid.value = _editUid;
  parentUid.value = _parentUid;
  parentRole.value = _parentRole;
  EditAgentRuleFormComponentKey.value++;
  SalesEditTopAgentFormComponentKey.value++;
  isLoading.value = false;
};

const hide = () => {
  hideModal(IBLinkDetailModalRef.value);
};

defineExpose({
  hide,
  show,
});
</script>

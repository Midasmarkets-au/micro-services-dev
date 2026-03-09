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
        <div v-if="isLoading">isLoading...</div>
        <div v-else class="form-outer">
          <SalesEditTopAgentForm
            v-if="isSales"
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
            :myRole="AccountRoleTypes.Sales"
            :parentUid="parentUid"
            :editUid="editUid"
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
import SalesEditTopAgentForm from "../form/SalesEditTopAgentForm.vue";
import EditAgentRuleForm from "@/projects/client/modules/ib/components/form/EditAgentRuleForm.vue";

const editUid = ref(0);
const parentUid = ref(0);
const isSales = ref(false);
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

const show = async (_isSales: any, _parentUid: number, _editUid: number) => {
  isLoading.value = true;
  showModal(IBLinkDetailModalRef.value);

  isSales.value = _isSales;
  editUid.value = _editUid;
  parentUid.value = _parentUid;
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

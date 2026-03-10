<template>
  <div
    class="modal fade"
    id="kt_modal_dashboard_notice"
    tabindex="-1"
    aria-hidden="true"
    ref="DashboardNoticeModalRef"
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
        <div v-if="!isLoading" class="form-outer">test</div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { showModal, hideModal } from "@/core/helpers/dom";

const isLoading = ref(true);
const DashboardNoticeModalRef = ref<null | HTMLElement>(null);

const props = defineProps<{
  productCategory: any;
}>();

const show = async () => {
  isLoading.value = true;
  showModal(DashboardNoticeModalRef.value);

  isLoading.value = false;
};

const hide = () => {
  hideModal(DashboardNoticeModalRef.value);
};

defineExpose({
  hide,
  show,
});
</script>

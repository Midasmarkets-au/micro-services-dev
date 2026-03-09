<!-- Hank Refactor on 02/01/2024 -->
<template>
  <div class="modal fade" ref="financialPopupCreateRef">
    <div class="modal-dialog modal-dialog-centered mw-750px">
      <div class="modal-content">
        <div class="form fv-plugins-bootstrap5 fv-plugins-framework">
          <!-- ============================================================================= -->
          <!------------------------------------------------------------------- Modal Header -->
          <!-- ============================================================================= -->
          <div class="modal-header">
            <h2
              class="fs-2"
              :class="{
                'fs-1': isMobile,
              }"
            >
              {{ $t("tip.financialPopup") }}
            </h2>
            <div data-bs-dismiss="modal">
              <span class="svg-icon svg-icon-1">
                <inline-svg src="/images/icons/arrows/arr061.svg" />
              </span>
            </div>
          </div>

          <!-- ============================================================================== -->
          <!------------------------------------------------------------------- Modal SideBar -->
          <!-- ============================================================================== -->
          <div class="p-9">
            <div class="h4 mb-9">{{ $t("tip.financialPopup1") }}</div>

            <ul class="fs-6">
              <li>
                {{ $t("tip.financialPopupBullet1") }}
              </li>
              <li>
                {{ $t("tip.financialPopupBullet2") }}
              </li>
              <li>
                {{ $t("tip.financialPopupBullet3") }}
              </li>
              <li>
                {{ $t("tip.financialPopupBullet4") }}
              </li>
              <li>
                {{ $t("tip.financialPopupBullet5") }}
              </li>
              <li>
                {{ $t("tip.financialPopupBullet6") }}
              </li>
            </ul>

            <div class="h5 mt-9 mb-3">{{ $t("tip.financialPopup2") }}</div>
            <div class="h5 mt-5 mb-3">{{ $t("tip.financialPopup3") }}</div>
          </div>

          <!-- ============================================================================== -->
          <!-------------------------------------------------------------------------- Footer -->
          <!-- ============================================================================== -->
          <div class="modal-footer">
            <div class="d-flex flex-stack">
              <div>
                <button
                  class="btn btn-primary text-uppercase"
                  @click="handleConfirm"
                >
                  {{ $t("action.next") }}
                </button>
              </div>
            </div>
          </div>
          <!-- ================================================================================== -->
          <!-------------------------------------------------------------------------- Footer END -->
          <!-- ================================================================================== -->
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { showModal, hideModal, removeModalBackdrop } from "@/core/helpers/dom";
import { ref } from "vue";

const { t } = useI18n();
const financialPopupCreateRef = ref<HTMLElement | null>(null);
let resolvePromise = null;

const show = async () => {
  showModal(financialPopupCreateRef.value);
  return new Promise((resolve) => {
    resolvePromise = resolve;
  });
};

const handleConfirm = () => {
  hideModal(financialPopupCreateRef.value);

  if (resolvePromise) {
    resolvePromise(true);
    resolvePromise = null;
  }
};

defineExpose({
  show,
});
</script>

<style scoped lang="scss">
.info-part {
  @media (max-width: 768px) {
    label {
      font-size: 16px;
    }
  }
}

.form-style {
  width: 100%;
  height: 400px;
  overflow-y: auto;
  border-left: 1px solid #e4e6ef;
  padding: 30px 30px;
  @media (max-width: 768px) {
    padding: 30px 20px;
  }
}
.fields-title {
  color: #0053ad;
  background-color: #f5f7fa;
  border-right: 1px solid #e4e6ef;
}
.stepper-icon-round {
  border-radius: 100% !important;
}

.createAccountTitle {
  color: #717171;
}

.alpha-tag {
  background: rgba(123, 97, 255, 0.1);
  color: #7b61ff;
  border-radius: 8px;
  padding: 0 8px;
}

.standard-tag {
  background-color: rgba(255, 205, 147, 0.3);
  color: #ff8a00;
  border-radius: 8px;
  padding: 0 8px;
}

.statusBadgeCompleted {
  background-color: rgba(97, 200, 166, 0.3);
  color: #009262;
  border-radius: 5px;
  padding: 5px 10px;
  font-weight: 700;
}

.account-review {
  width: 70%;
  border-radius: 30px;
  border: 1px solid #e4e6ef;
  overflow: hidden;
  margin: auto;

  @media (max-width: 768px) {
    width: 100%;
  }
}
</style>

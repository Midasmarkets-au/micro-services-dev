<template>
  <div
    class="modal fade"
    tabindex="-1"
    aria-hidden="true"
    aria-modal="true"
    ref="newTargetModalRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-sm-800px mw-750px">
      <div class="modal-content rounded-3" v-if="details != null">
        <div
          class="form fv-plugins-bootstrap5 fv-plugins-framework w-100 max-height"
          id="kt_modal_add_event_form"
          :rules="rules"
        >
          <!-- header section -->
          <div class="modal-header border-0 px-sm-10 px-5 pb-3">
            <div class="d-flex gap-4 align-items-baseline">
              <h2 class="fw-bold item-title fs-2">
                {{ $t("fields.giftRedemption") }}
              </h2>
              <h4 class="credit-notice ms-10 fw-bold" v-if="!isMobile">
                {{ $t("fields.currentPoints") }}: {{ userDetail.point }}
              </h4>
            </div>
            <div id="kt_modal_add_event_close" data-bs-dismiss="modal">
              <span class="svg-icon svg-icon-1">
                <inline-svg src="/images/icons/arrows/arr061.svg" />
              </span>
            </div>
          </div>
          <!-- content section -->
          <div class="w-100 d-flex flex-sm-row flex-column p-sm-10 p-5 gap-10">
            <div>
              <!-- <img :src="image" class="border rounded shop-item-image" /> -->
              <el-image :src="image" class="border rounded shop-item-image">
                <template #error>
                  <div class="image-slot">
                    <el-icon><icon-picture /></el-icon>
                  </div> </template
              ></el-image>
            </div>
            <div class="w-100">
              <h4 class="item-title fs-2">{{ details.name }}</h4>
              <div
                class="item-title_subtitle fs-7"
                v-html="details.description"
              ></div>
              <span class="credit-notice my-1 fs-2"
                >{{ details.point }} {{ $t("fields.points") }}</span
              >
              <div class="my-1">
                <span class="credit-notice fs-2 mb-5" v-if="isMobile">
                  {{ $t("fields.currentPoints") }}: {{ userDetail.point }}
                </span>
              </div>
              <div class="d-flex flex-column gap-2">
                <div class="item-title_subtitle fs-6">
                  {{ $t("fields.quantity") }}
                </div>
                <div class="d-flex flex-row align-items-center gap-1">
                  <button
                    class="amount-button btn btn-lg btn-link d-flex align-items-center justify-content-center fs-4"
                    style="border-radius: 4px"
                    @click="handleAmount('min')"
                    :disabled="isRegistering"
                  >
                    -
                  </button>
                  <label
                    class="d-flex align-items-center justify-content-center amount-label"
                    >{{ quantity }}</label
                  >
                  <button
                    class="amount-button btn btn-lg btn-link d-flex align-items-center justify-content-center fs-4"
                    style="border-radius: 4px"
                    @click="handleAmount('add')"
                    :disabled="isRegistering"
                  >
                    +
                  </button>
                </div>
              </div>
              <div class="d-flex flex-column gap-2 my-1">
                <div class="item-title_subtitle">
                  {{ $t("fields.address") }}
                </div>
                <NewAddressCard ref="addressRef" />
              </div>
              <div class="d-flex flex-column gap-2">
                <div class="item-title_subtitle">
                  {{ $t("fields.comment") }}
                </div>
                <textarea
                  class="form-control fs-8 fs-sm-6"
                  rows="3"
                  :disabled="isRegistering"
                  :placeholder="$t('fields.comment')"
                  v-model="comment"
                ></textarea>
              </div>
            </div>
          </div>
          <div
            class="d-flex flex-sm-row flex-column justify-content-between align-items-sm-center align-items-start w-100 pb-10 pe-sm-10 gap-sm-0 gap-3"
          >
            <!-- checkbox section -->
            <div
              class="d-flex align-items-center gap-2 mx-sm-10 mx-6"
              :class="isMobile ? 'flex-column mb-5 mt-2' : 'flex-row'"
            >
              <div class="d-flex align-items-center gap-2">
                <input
                  class=""
                  :checked="checkedRef"
                  :disabled="isRegistering"
                  type="checkbox"
                  name="user_management_read"
                  @change="(e) => handleChange(e)"
                />
                <span>{{ $t("fields.iAgreeToEventRules") }}.</span>
              </div>
              <a href="" target="_blank"
                ><router-link to="/eventshop/rules" target="_blank"
                  >{{ $t("action.clickHereToViewEventRules") }}
                </router-link></a
              >
            </div>
            <div class="d-flex align-items-center gap-3 flex-row ms-sm-0 ms-5">
              <button
                class="btn btn-sm btn-secondary"
                @click="hide()"
                :disabled="isRegistering"
              >
                {{ $t("action.cancel") }}
              </button>
              <!-- agree button -->
              <div class="d-flex align-items-center justify-content-end">
                <LoadingButton
                  :is-loading="isRegistering"
                  :save-title="
                    $t('action.redeem') +
                    ' ' +
                    totalPoints +
                    ' ' +
                    $t('fields.points')
                  "
                  :saved-title="$t('fields.processing')"
                  class="btn btn-sm btn-primary d-flex align-items-center gap-2"
                  @click.prevent="handleRegister"
                  :disabled="!canRegister || isRegistering"
                />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, inject, watch } from "vue";
import { hideModal, showModal } from "@/core/helpers/dom";
import LoadingButton from "@/components/buttons/LoadingButton.vue";
import NewAddressCard from "./NewAddressCard.vue";
import ShopService from "../../services/ShopService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import i18n from "@/core/plugins/i18n";
import ClientGlobalInjectionKeys from "@/core/types/ClientGlobalInjectionKeys";
import Decimal from "decimal.js";
import { isMobile } from "@/core/config/WindowConfig";
import { Picture as IconPicture } from "@element-plus/icons-vue";
const { t } = i18n.global;
const newTargetModalRef = ref<null | HTMLElement>(null);
const rules = ref();
const details = ref<any>(null);
const checkedRef = ref<boolean>(false);
const canRegister = ref<boolean>(false);
const addressRef = ref<any>(null);
const image = ref("");
const isRegistering = ref(false);
const quantity = ref(1);
const comment = ref("");
const userDetail = inject<any>(
  ClientGlobalInjectionKeys.EVENT_SHOP_USER_DETAIL
);

const totalPoints = computed(() => details.value?.point * quantity.value);
const show = async (_detail: any) => {
  checkedRef.value = false;
  isRegistering.value = false;
  await fecthData(_detail);
  fetchImage();
  showModal(newTargetModalRef.value);
};
const fecthData = async (item: any) => {
  try {
    const res = await ShopService.getItemDetail(item.hashId);
    details.value = res;
    details.value.point = new Decimal(details.value.point)
      .div(10000)
      .toNumber();
  } catch (error: any) {
    console.log(error);
    MsgPrompt.error(t("status.failed"));
  }
};

const fetchImage = async () => {
  if (details.value.images.length > 0) {
    const imageUrl = await ShopService.getImagesWithGuid(
      details.value.images[0]
    );
    image.value = imageUrl;
  }
};
const handleRegister = async () => {
  isRegistering.value = true;
  if (!checkAddress()) {
    MsgPrompt.error(t("title.addAddress"));
    isRegistering.value = false;
    return;
  }
  try {
    isRegistering.value = false;
    await ShopService.purchaseItem({
      shopItemHashId: details.value.hashId,
      quantity: quantity.value,
      addressHashId: addressRef.value.addressHashId,
      comment: comment.value,
    }).then(() => {
      MsgPrompt.success(t("status.success"));
      userDetail.value.point = userDetail.value.point - totalPoints.value;
      hideModal(newTargetModalRef.value);
    });
  } catch (error: any) {
    console.log(error);
    MsgPrompt.error(t("status.failed"));
  }
};

const checkAddress = () => {
  if (addressRef.value.addressHashId) {
    return true;
  }
  return false;
};

const handleAmount = (type: string) => {
  if (type === "add") {
    quantity.value += 1;
  } else {
    quantity.value < 2 ? (quantity.value = 1) : (quantity.value -= 1);
  }
};
const handleChange = (e: Event) => {
  if (e instanceof Event) {
    const inputElement = e.target as HTMLInputElement;
    checkedRef.value = inputElement.checked;
  }
};
const hide = () => {
  checkedRef.value = false;
  isRegistering.value = false;
  hideModal(newTargetModalRef.value);
};

watch([checkedRef, totalPoints], ([check, total]) => {
  if (check && userDetail.value.point >= total) {
    canRegister.value = true;
  } else {
    canRegister.value = false;
  }
});
defineExpose({
  show,
  hide,
});
</script>

<style lang="scss">
.item-title {
  font-weight: 600;
  line-height: 28px;
  color: #212121;
  &_subtitle {
    font-weight: 400;
    line-height: 20px;
    color: #212121;
  }
}
.credit-notice {
  font-weight: 400;
  line-height: 28px;
  color: #f7b93f;
}
.secondary-title {
  font-weight: 400;
  line-height: 20px;
  color: #000000;
}
.amount-label {
  width: 48px;
  height: 25px;
  background: #f5f7fa;
  border-radius: 4px;
  line-height: 20px;
}
.amount-button {
  width: 24px !important;
  height: 24px !important;
  background: #f5f7fa;
  border-radius: 4px;
  &:hover {
    background: #ced1d4;
  }
  &:focus {
    background: #ced1d4;
  }
  &:active {
    background: #ced1d4;
  }
}
.shop-item-image {
  object-fit: cover;
  width: 168px;
  height: 168px;
  @media screen and (max-width: 768px) {
    width: 305px;
    height: 305px;
  }
}
.max-height {
  max-height: 80vh;
  overflow: auto;
}
@media screen and (max-width: 768px) {
  .max-height {
    max-height: 90vh;
  }
}
</style>

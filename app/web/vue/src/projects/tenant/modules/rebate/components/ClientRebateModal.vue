<template>
  <div
    class="modal fade"
    id="kt_modal_create_deposit"
    tabindex="-1"
    aria-hidden="true"
    ref="linkExchangeRateModalRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-900px">
      <div class="modal-content">
        <div class="form fv-plugins-bootstrap5 fv-plugins-framework">
          <div class="modal-header">
            <h2 class="fw-bold">Exchange Rate Update</h2>
          </div>

          <div class="p-9">
            <div class="row mb-9">
              <div class="col-6 d-flex flex-column">
                <label for="" class="required mb-3">Exchange Rate</label>
                <Field
                  name="exchangeRateId"
                  class="form-select form-select-solid"
                  as="select"
                  v-model.number="updateForm.exchangeRateId"
                >
                  <option value="">Select a Exchange Rate</option>
                  <option
                    v-for="(item, index) in exchangeRates.data"
                    :label="item.name"
                    :key="index"
                    :value="item.id"
                  ></option>
                </Field>
              </div>

              <div class="col-6 d-flex flex-column">
                <label for="" class="required mb-3">Rate</label>
                <Field
                  type="text"
                  class="form-control form-control-solid"
                  name="rate"
                  v-model="updateForm.rate"
                />
              </div>
            </div>

            <hr v-if="exist" />
            <div>
              {{ updateForm.exchangeRate }}
            </div>
          </div>

          <div class="modal-footer">
            <!--begin::Actions-->
            <div class="d-flex flex-stack">
              <!--begin::Wrapper-->
              <div class="me-2">
                <button
                  type="button"
                  class="btn btn-lg btn-light-danger me-3"
                  @click="hide()"
                >
                  Cancel
                </button>
              </div>
              <!--end::Wrapper-->
              <!--begin::Wrapper-->
              <div>
                <button class="btn btn-primary" @click="updateExRate">
                  Update
                </button>
              </div>
              <!--end::Wrapper-->
            </div>
            <!--end::Actions-->
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, nextTick } from "vue";
import { showModal, hideModal } from "@/core/helpers/dom";
import { Field, ErrorMessage, useForm } from "vee-validate";
import RebateService from "../services/RebateService";

const isLoading = ref(false);
const exist = ref(true);
const sid = ref<number>();
const linkExchangeRateModalRef = ref<null | HTMLElement>(null);
const updateForm = ref<any>({});
const exchangeRates = ref<any>({});

const emits = defineEmits<{
  (e: "refresh"): void;
}>();

const reset = () => {
  isLoading.value = false;
};

const show = async (_sid: number, _item: any) => {
  isLoading.value = true;
  updateForm.value = {};
  sid.value = _sid;

  reset();
  await nextTick();

  if (_item == null) {
    exist.value = false;
  } else {
    const response = await RebateService.getRebatePipExLink(_item.id);
    updateForm.value = response;
    exist.value = true;
  }

  try {
    exchangeRates.value = await RebateService.getExchangeRate();
  } catch (error) {
    console.log(error);
  }

  isLoading.value = false;
  showModal(linkExchangeRateModalRef.value);
};

const updateExRate = async () => {
  isLoading.value = true;

  try {
    if (exist.value) {
      await RebateService.putRebatePipExLink(updateForm.value.id, {
        id: updateForm.value.id,
        rate: updateForm.value.rate,
        exchangeRateId: updateForm.value.exchangeRateId,
      });
    } else {
      await RebateService.postRebatePipExLink({
        symbolId: sid.value,
        rate: updateForm.value.rate,
        exchangeRateId: updateForm.value.exchangeRateId,
      });
    }
    hide();
    emits("refresh");
  } catch (error) {
    console.log(error);
  }
};

const hide = () => {
  hideModal(linkExchangeRateModalRef.value);
};

defineExpose({
  hide,
  show,
});
</script>

<style scoped></style>

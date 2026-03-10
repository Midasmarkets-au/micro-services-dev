<template>
  <div
    class="modal fade exchangeModalHistoryTable"
    id="kt_modal_create_deposit"
    tabindex="-1"
    aria-hidden="true"
    ref="exchangeRateModalRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-900px">
      <div class="modal-content">
        <div class="form fv-plugins-bootstrap5 fv-plugins-framework">
          <div class="modal-header">
            <h2 class="fw-bold">
              <span v-if="isUpdate">Update Exchange Rate</span>
              <span v-else>Create Exchange Rate</span>
            </h2>
          </div>

          <div class="p-9">
            <div v-if="!isUpdate" class="row mb-9">
              <div class="col-6 d-flex flex-column">
                <div class="d-flex flex-column mb-5 fv-row">
                  <label
                    class="d-flex align-items-center fs-5 fw-semobold mb-2"
                  >
                    <span class="required">From Currency</span>
                  </label>

                  <Field
                    name="currencyId"
                    class="form-select form-select-solid"
                    as="select"
                    v-model.number="updateForm.fromCurrencyId"
                  >
                    <option value="">
                      {{ $t("tip.selectCurrency") }}
                    </option>
                    <option
                      v-for="(
                        { label, value }, index
                      ) in ConfigCurrencySelections"
                      :label="label"
                      :key="index"
                      :value="value"
                    ></option>
                  </Field>
                </div>
              </div>

              <div class="col-6 d-flex flex-column">
                <div class="d-flex flex-column mb-5 fv-row">
                  <label
                    class="d-flex align-items-center fs-5 fw-semobold mb-2"
                  >
                    <span class="required">To Currency</span>
                  </label>

                  <Field
                    name="currencyId"
                    class="form-select form-select-solid"
                    v-model.number="updateForm.toCurrencyId"
                    as="select"
                  >
                    <option value="">
                      {{ $t("tip.selectCurrency") }}
                    </option>
                    <option
                      v-for="(
                        { label, value }, index
                      ) in ConfigCurrencySelections"
                      :label="label"
                      :key="index"
                      :value="value"
                    ></option>
                  </Field>
                </div>
              </div>
            </div>

            <div v-else class="row mb-9">
              <div class="col-6 d-flex flex-column">
                <label for="" class="required mb-3">From Currency</label>
                <Field
                  type="text"
                  class="form-control form-control-solid"
                  name="fromCurrencyName"
                  v-model="updateForm.fromCurrencyName"
                  :disabled="isUpdate"
                />
              </div>

              <div class="col-6 d-flex flex-column">
                <label for="" class="required mb-3">To Currency</label>
                <Field
                  type="text"
                  class="form-control form-control-solid"
                  name="toCurrencyName"
                  v-model="updateForm.toCurrencyName"
                  :disabled="isUpdate"
                />
              </div>
            </div>

            <div class="row mb-9">
              <div class="col-6 d-flex flex-column">
                <label for="" class="required mb-3">Name</label>
                <Field
                  type="text"
                  class="form-control form-control-solid"
                  name="name"
                  v-model="updateForm.name"
                />
                <ErrorMessage
                  class="fv-plugins-message-container invalid-feedback"
                  name="name"
                  as="div"
                >
                  {{ $t("tips.requiredField") }}</ErrorMessage
                >
              </div>

              <div class="col-6 d-flex flex-column">
                <label for="" class="required mb-3">Adjust Rate</label>
                <Field
                  type="text"
                  class="form-control form-control-solid"
                  name="adjustRate"
                  v-model="updateForm.adjustRate"
                />
                <ErrorMessage
                  class="fv-plugins-message-container invalid-feedback"
                  name="adjustRate"
                  as="div"
                >
                  {{ $t("tips.requiredField") }}</ErrorMessage
                >
              </div>
            </div>

            <div class="row">
              <div class="col-6 d-flex flex-column">
                <label for="" class="required mb-3">Buying Rate</label>
                <Field
                  type="text"
                  class="form-control form-control-solid"
                  name="buyingRate"
                  v-model="updateForm.buyingRate"
                />
                <ErrorMessage
                  class="fv-plugins-message-container invalid-feedback"
                  name="buyingRate"
                  as="div"
                >
                  {{ $t("tips.requiredField") }}</ErrorMessage
                >
              </div>

              <div class="col-6 d-flex flex-column">
                <label for="" class="required mb-3">Selling Rate</label>
                <Field
                  type="text"
                  class="form-control form-control-solid"
                  name="sellingRate"
                  v-model="updateForm.sellingRate"
                />
                <ErrorMessage
                  class="fv-plugins-message-container invalid-feedback"
                  name="sellingRate"
                />
              </div>
            </div>

            <div v-if="isUpdate">
              <div class="fs-2 mt-9">History</div>
              <hr />
              <table
                class="table align-middle table-row-dashed fs-6 gy-5"
                id="table_accounts_requests"
                style="width: 100%"
              >
                <thead>
                  <tr
                    class="text-start text-muted fw-bold fs-7 text-uppercase gs-0"
                  >
                    <th>Name</th>
                    <th>Buy</th>
                    <th>Sell</th>
                    <th>Adjust</th>
                    <th>Who</th>
                    <th>Date</th>
                    <th>Time</th>
                  </tr>
                </thead>

                <tbody v-if="isLoading">
                  <tr>
                    <td colspan="12"><scale-loader></scale-loader></td>
                  </tr>
                </tbody>
                <tbody v-else-if="!isLoading && dataHistory.length === 0">
                  <tr>
                    <td colspan="12">{{ $t("tip.nodata") }}</td>
                  </tr>
                </tbody>
                <tbody
                  v-else-if="!isLoading && dataHistory.length != 0"
                  name="table-delete-fade"
                  class="table-delete-fade-container text-gray-600 fw-semibold"
                >
                  <tr v-for="(item, index) in dataHistory" :key="index">
                    <td>{{ updateForm.name }}</td>
                    <td>{{ item.changes.originalValues.BuyingRate }}</td>
                    <td>{{ item.changes.originalValues.SellingRate }}</td>
                    <td>{{ item.changes.originalValues.AdjustRate }}</td>
                    <td>{{ item.partyId }}</td>
                    <td>
                      {{ item.changes.originalValues.UpdatedOn.split(" ")[0] }}
                    </td>
                    <td>
                      {{ item.changes.originalValues.UpdatedOn.split(" ")[1] }}
                    </td>
                  </tr>
                </tbody>

                <div
                  class="mt-15 fs-5 text-center text-gray-600"
                  v-if="!isLoading && dataHistory.length > 5"
                >
                  Scroll for More
                </div>
              </table>
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
                <button
                  class="btn btn-primary"
                  @click="updateExRate"
                  :disabled="isLoading"
                >
                  <span v-if="isLoading">
                    {{ $t("action.waiting") }}
                    <span
                      class="spinner-border h-15px w-15px align-middle text-gray-400"
                    ></span>
                  </span>

                  <span v-else>
                    <span v-if="isUpdate">Update</span>
                    <span v-else>Create</span>
                  </span>
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
import { Field, ErrorMessage } from "vee-validate";
import RebateService from "../services/RebateService";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import { showModal, hideModal } from "@/core/helpers/dom";
import { ConfigCurrencySelections } from "@/core/types/CurrencyTypes";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const isLoading = ref(true);
const isUpdate = ref(true);
const updateForm = ref<any>({});
const dataHistory = ref<any>([]);
const currencySelections = ref(Array<any>());
const exchangeRateModalRef = ref<null | HTMLElement>(null);

const emits = defineEmits<{
  (e: "refresh"): void;
}>();

const reset = () => {
  isLoading.value = true;
};

const show = async (_isUpdate: boolean, _item?: any) => {
  reset();
  await nextTick();
  showModal(exchangeRateModalRef.value);

  isLoading.value = true;
  isUpdate.value = _isUpdate;
  updateForm.value = {};

  if (_isUpdate) {
    updateForm.value = JSON.parse(JSON.stringify(_item));

    try {
      dataHistory.value = await RebateService.getExchangeRateHistory(_item?.id);
      isLoading.value = false;
    } catch (error) {
      // console.log(error);
    }
  } else {
    isLoading.value = false;
  }
};

const updateExRate = async () => {
  isLoading.value = true;

  try {
    if (isUpdate.value) {
      await RebateService.putExchangeRate(updateForm.value.id, {
        id: updateForm.value.id,
        name: updateForm.value.name,
        buyingRate: updateForm.value.buyingRate,
        sellingRate: updateForm.value.sellingRate,
        adjustRate: updateForm.value.adjustRate,
      });
    } else {
      await RebateService.postExchangeRate({
        fromCurrencyId: updateForm.value.fromCurrencyId,
        toCurrencyId: updateForm.value.toCurrencyId,
        name: updateForm.value.name,
        buyingRate: updateForm.value.buyingRate,
        sellingRate: updateForm.value.sellingRate,
        adjustRate: updateForm.value.adjustRate,
      });
    }

    hide();
    emits("refresh");
  } catch (error) {
    MsgPrompt.error(error).then(() => {
      isLoading.value = false;
    });
  }
};

const hide = () => {
  hideModal(exchangeRateModalRef.value);
};

defineExpose({
  hide,
  show,
});
</script>

<style scoped>
tbody {
  display: block;
  height: 250px;
  overflow: auto;
}

tr {
  display: table;
  width: 100%;
  height: 50px;
  table-layout: fixed; /* even columns width , fix width of table too*/
}
</style>

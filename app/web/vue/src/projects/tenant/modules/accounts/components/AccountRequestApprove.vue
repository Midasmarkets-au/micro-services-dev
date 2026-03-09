<template>
  <ModelConfirm
    :ok="ok"
    :title="$t('action.approve')"
    elId="AccountRequestApprove"
    :submited="submited"
    :submitTitle="$t('action.saving')"
    ref="refAccountRequestApproveModel"
  >
    <div>
      <div class="fv-row mb-7"></div>
    </div>
  </ModelConfirm>
</template>
<script lang="ts" setup>
// import ApiService from "@/core/services/ApiService";
import { ref, inject } from "vue";
import ModelConfirm from "@/components/ModelConfirm.vue";
import ErrorMsg from "@/components/ErrorMsg";
import { apiProviderKey } from "@/core/plugins/providerKeys";

const api = inject(apiProviderKey);

const refAccountRequestApproveModel = ref(null);

const submited = ref(true);

const data = ref({});

const emit = defineEmits(["approved"]);

const ok = async () => {
  submited.value = true;
  await api["account.applications.approve"]({
    id: data.value.id,
    data: {
      status: "approved",
    },
  })
    .then(({ data }) => {
      submited.value = false;
      emit("aprove", data);
      refAccountRequestApproveModel.value.hide();
    })
    .catch(({ response }) => {
      console.log(response);
      ErrorMsg.show(response);
      submited.value = false;
    });
};

const show = (_data) => {
  refAccountRequestApproveModel.value.show();
  data.value = _data;
  submited.value = false;
};
defineExpose({
  show,
});
</script>

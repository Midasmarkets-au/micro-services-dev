<template>
  <SimpleForm
    ref="modalRef"
    :title="'Special Level Setting'"
    :is-loading="isLoading"
    :submit="submit"
    :before-close="hide"
    :width="900"
  >
    <div class="text-center" v-if="ruleId == undefined" style="color: #900000">
      沒有紀錄! 此操作為新增 rebateAgentRule! 請檢查數據庫!
    </div>
    <div class="text-end mt-5 mb-5">
      Format Example: [ { "cid": 1, "r": 8 }, { "cid": 2, "r": 12 } ]
    </div>
    <div class="d-flex">
      <div style="width: 115px">Account Type</div>
      <el-input v-model="accountType" placeholder="Please input account type" />
    </div>
    <div v-if="ruleId == undefined" class="my-4">
      <el-form-item
        label="Schema Items"
        label-position="right"
        label-width="100px"
        ><el-input label="value" v-model="newSchema" type="textarea"></el-input
      ></el-form-item>
    </div>
    <div class="my-4">
      <el-form-item
        label="Level Setting Items"
        label-position="right"
        label-width="100px"
        ><el-input
          label="value"
          v-model="newLevelSetting"
          type="textarea"
        ></el-input
      ></el-form-item>
    </div>

    <div class="d-flex justify-content-end">
      <el-switch
        v-model="unlockClearBtn"
        active-text="清除就没啰"
        inactive-text="别乱点"
        class="p-4"
        size="large"
        style="font-size: 18px"
      >
      </el-switch>

      <button
        class="btn btn-sm btn-light-danger border-0"
        @click="clearLevelSetting()"
        :disabled="!unlockClearBtn"
      >
        Clear Level Setting
      </button>
    </div>
  </SimpleForm>
</template>
<script setup lang="ts">
import { ref } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import AccountService from "../../services/AccountService";

const unlockClearBtn = ref(false);
const isLoading = ref(false);
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const newLevelSetting = ref<any>("");
const newSchema = ref<any>("");
const ruleId = ref(0);
const accountType = ref("");
const agentAccountId = ref(0);

const emit = defineEmits(["prepareRebateData"]);

const hide = () => {
  modalRef.value?.hide();
};

const show = async (_ruleId: any, _accountType: any, _agentAccountId?: any) => {
  console.log(_ruleId, _accountType);
  ruleId.value = _ruleId;
  accountType.value = _accountType;
  agentAccountId.value = _agentAccountId;
  modalRef.value?.show();
};

const clearLevelSetting = async () => {
  try {
    if (unlockClearBtn.value) {
      await AccountService.clearLevelSetting(ruleId.value);
      emit("prepareRebateData");
      hide();
    }
  } catch (e) {
    console.log(e);
  }
};

const submit = async () => {
  // let obj = JSON.parse(newLevelSetting.value);
  // let requestObject = Object.keys(obj).map((key) => ({
  //   cid: parseInt(key),
  //   r: obj[key],
  // }));

  try {
    if (ruleId.value == undefined) {
      await AccountService.postRebateAgentRule({
        agentAccountId: agentAccountId.value,
        schema: [
          {
            accountType: accountType.value,
            allowCommissions: [],
            allowPips: [],
            commission: 0,
            pips: 0,
            percentage: 100,
            items: JSON.parse(newSchema.value),
          },
        ],
        levelSetting: {
          allowedAccounts: [
            {
              accountType: accountType.value,
              allowCommissions: [],
              allowPips: [],
              commission: 0,
              pips: 0,
              percentage: 0,
              items: JSON.parse(newLevelSetting.value),
            },
          ],
          language: "en-us",
        },
      });
    } else {
      await AccountService.updateLevelSettingItems(ruleId.value, {
        accountType: accountType.value,
        allowCommissions: [],
        allowPips: [],
        commission: 0,
        pips: 0,
        percentage: 0,
        items: JSON.parse(newLevelSetting.value),
      });
    }
    emit("prepareRebateData");
    hide();
  } catch (e) {
    console.log(e);
  }
};
defineExpose({
  show,
  hide,
});
</script>

<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <el-input
          v-model="criteria.event"
          :placeholder="$t('fields.event')"
          :disabled="isLoading"
          clearable
          @keyup.enter="fetchData(1)"
        />
        <el-input
          v-model="criteria.eventId"
          :placeholder="$t('fields.eventId')"
          :disabled="isLoading"
          class="ms-4"
          clearable
          @keyup.enter="fetchData(1)"
        />
        <el-input
          v-model="criteria.receiver"
          :placeholder="$t('fields.email')"
          :disabled="isLoading"
          class="ms-4"
          clearable
          @keyup.enter="fetchData(1)"
        />
        <el-input
          v-model="criteria.receiverPartyId"
          placeholder="Party ID"
          :disabled="isLoading"
          class="ms-4"
          clearable
          @keyup.enter="fetchData(1)"
        />
        <el-input
          v-model="criteria.method"
          :placeholder="$t('fields.method')"
          :disabled="isLoading"
          class="ms-4"
          clearable
          @keyup.enter="fetchData(1)"
        />
        <el-select
          v-model="criteria.status"
          :disabled="isLoading"
          :placeholder="$t('fields.status')"
          class="ms-4"
          clearable
        >
          <el-option
            v-for="item in messageRecordStatusOptions"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          />
        </el-select>
        <el-button
          type="warning"
          @click="fetchData(1)"
          plain
          :loading="isLoading"
          class="ms-4"
          >{{ $t("action.search") }}</el-button
        >

        <el-button @click="reset" :disabled="isLoading" class="ms-4">
          {{ $t("action.clear") }}
        </el-button>
      </div>
    </div>
    <div class="card-body">
      <table class="table table-tenant table-hover">
        <thead>
          <tr>
            <th>ID</th>
            <th>Party ID</th>
            <th>{{ $t("fields.receiver") }}</th>
            <th>{{ $t("fields.receiverName") }}</th>
            <th>{{ $t("fields.method") }}</th>
            <th>{{ $t("fields.eventId") }}</th>
            <th>{{ $t("fields.event") }}</th>
            <th>{{ $t("fields.status") }}</th>
            <th>{{ $t("fields.note") }}</th>
            <th>{{ $t("fields.createdOn") }}</th>
            <th>{{ $t("action.action") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading" style="height: 300px">
          <tr>
            <td colspan="12">
              <scale-loader :color="'#ffc730'"></scale-loader>
            </td>
          </tr>
        </tbody>
        <tbody v-else-if="!isLoading && data.length == 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr
            v-for="(item, index) in data"
            :key="index"
            :class="{
              'tr-select': item.id === accountSelected,
            }"
            @click="selectedAccount(item.id)"
          >
            <td>{{ item.id }}</td>
            <td>{{ item.receiverPartyId }}</td>
            <td>{{ item.receiver }}</td>
            <td>{{ item.receiverName }}</td>
            <td>{{ item.method }}</td>
            <td>{{ item.eventId }}</td>
            <td>{{ item.event }}</td>
            <td>{{ $t(`status.${MessageRecordStatus[item.status]}`) }}</td>
            <td>{{ item.note }}</td>
            <td>
              <TimeShow type="GMToneLiner" :date-iso-string="item.createdOn" />
            </td>
            <td>
              <el-button
                type="warning"
                plain
                size="small"
                @click="view(item)"
                >{{ $t("action.view") }}</el-button
              >
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
  </div>
  <recordDetail ref="recordDetailRef" />
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import ReportService from "../services/ReportService";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import recordDetail from "../components/messageRecord/recordDetail.vue";
import {
  MessageRecordStatus,
  messageRecordStatusOptions,
} from "@/core/types/messageRecord/messageRecordStatus";

const isLoading = ref(false);
const data = ref<any>([]);
const recordDetailRef = ref<any>(null);
const accountSelected = ref(0);

const criteria = ref<any>({
  page: 1,
  size: 20,
});

const view = (item: any) => {
  recordDetailRef.value.show(item);
};

const reset = async () => {
  criteria.value = {
    page: 1,
    size: 20,
  };
  await fetchData(1);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await ReportService.queryMessageRecords(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};
const selectedAccount = (id: number) => {
  accountSelected.value = id;
};
onMounted(() => {
  fetchData(1);
});
</script>

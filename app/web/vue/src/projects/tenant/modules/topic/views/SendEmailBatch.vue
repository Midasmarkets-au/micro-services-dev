<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <el-button
          type="success"
          @click="createBatchRef.show()"
          :disabled="isLoading"
          >Create Batch</el-button
        >

        <el-button type="primary" @click="fetchData" :disabled="isLoading"
          >Refresh</el-button
        >
      </div>
    </div>
    <div class="card-body">
      <div class="d-flex justify-content-between">
        <div>
          <div v-if="data.contents">
            Title: {{ data?.contents["en-us"]?.title }}
          </div>
          <div>Topic Key: {{ data.topicKey }}</div>
          <div>uuid: {{ data.uuid }}</div>
          <div>Total Emails: {{ data.total }}</div>
          <div>Status: {{ data.status }}</div>
          <div>
            <div>Sent: {{ data.sent }}</div>
            <div>Failed: {{ data.failed }}</div>
            <div>Failed Email : {{ data.failedEmails }}</div>
          </div>
          <div>
            <el-button
              type="primary"
              @click="showDetails"
              :loading="isLoadingDetail"
              >details</el-button
            >
          </div>
        </div>
        <div>
          <el-button type="warning" @click="createBatchRef.show(data)"
            >Edit</el-button
          >
          <el-button type="success" @click="sendEmail" :disabled="isLoading"
            >Sent Emails</el-button
          >
        </div>
      </div>
      <el-divider></el-divider>
      <div class="mt-6">
        <el-input
          :disabled="isLoading"
          class="w-500px me-6"
          v-model="receiverEmails"
          :autosize="{ minRows: 2, maxRows: 4 }"
          type="textarea"
          placeholder="Enter email addresses separated by comma"
        />
        <el-button type="warning" @click="sendTestEmail" :loading="isLoading"
          >Send Test Email</el-button
        >
      </div>
    </div>
  </div>
  <CreateBatch ref="createBatchRef" @event-submit="fetchData" />

  <!-- Details Dialog -->
  <el-dialog
    v-model="dialogVisible"
    :title="$t('title.emailBatchDetails')"
    width="900px"
  >
    <!-- Statistics Cards -->
    <div class="row mb-5">
      <div class="col-md-6">
        <div class="card card-flush h-100">
          <div class="card-body d-flex flex-column justify-content-center">
            <div class="d-flex align-items-center">
              <span class="fs-2hx fw-bold text-success me-2">{{
                successCount
              }}</span>
              <span class="fs-6 text-gray-600">{{
                $t("title.successCount")
              }}</span>
            </div>
          </div>
        </div>
      </div>
      <div class="col-md-6">
        <div class="card card-flush h-100">
          <div class="card-body d-flex flex-column justify-content-center">
            <div class="d-flex align-items-center">
              <span class="fs-2hx fw-bold text-danger me-2">{{
                failedCount
              }}</span>
              <span class="fs-6 text-gray-600">{{
                $t("title.failedCount")
              }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Search and Filter -->
    <div class="row mb-4">
      <div class="col-md-8">
        <el-input
          v-model="searchKeyword"
          :placeholder="$t('title.searchByEmail')"
          clearable
          prefix-icon="el-icon-search"
        />
      </div>
      <div class="col-md-4">
        <el-select v-model="failFilter" class="w-100">
          <el-option :label="$t('status.all')" value="all" />
          <el-option :label="$t('title.failedOnly')" value="failed" />
        </el-select>
      </div>
    </div>

    <!-- Table -->
    <el-table
      :data="filteredDetailData"
      border
      style="width: 100%"
      max-height="500px"
    >
      <el-table-column
        prop="email"
        :label="$t('fields.email')"
        min-width="200"
      />
      <el-table-column
        prop="isSuccess"
        :label="$t('status.success')"
        width="100"
        align="center"
      >
        <template #default="scope">
          <el-tag :type="scope.row.isSuccess === '1' ? 'success' : 'info'">
            {{ scope.row.isSuccess === "1" ? "Yes" : "No" }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column
        prop="isFail"
        :label="$t('status.failed')"
        width="100"
        align="center"
      >
        <template #default="scope">
          <el-tag :type="scope.row.isFail === '1' ? 'danger' : 'info'">
            {{ scope.row.isFail === "1" ? "Yes" : "No" }}
          </el-tag>
        </template>
      </el-table-column>
    </el-table>

    <template #footer>
      <span class="dialog-footer">
        <el-button @click="dialogVisible = false">{{
          $t("action.close")
        }}</el-button>
      </span>
    </template>
  </el-dialog>
</template>
<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import TopicService from "../services/TopicService";
import CreateBatch from "../components/sendEmailBatch/CreateBatch.vue";

const data = ref<any>({});
const isLoading = ref(false);
const isLoadingDetail = ref(false);
const createBatchRef = ref<any>(null);
const receiverEmails = ref(
  // "dealing@bacera.com,kay.wu@bacera.com,jiehe@bacera.com"
  "dealing@midasmkts.com,tech@midasmkts.com"
);
const dialogVisible = ref(false);
const detailData = ref<any[]>([]);
const searchKeyword = ref("");
const failFilter = ref("all"); // all, failed

// 计算成功数和失败数
const successCount = computed(() => {
  return detailData.value.filter((item) => item.isSuccess === "1").length;
});

const failedCount = computed(() => {
  return detailData.value.filter((item) => item.isFail === "1").length;
});

// 过滤后的表格数据
const filteredDetailData = computed(() => {
  let filtered = detailData.value;

  // 按失败状态筛选
  if (failFilter.value === "failed") {
    filtered = filtered.filter((item) => item.isFail === "1");
  }

  // 按邮箱关键词搜索
  if (searchKeyword.value.trim()) {
    const keyword = searchKeyword.value.toLowerCase().trim();
    filtered = filtered.filter((item) =>
      item.email.toLowerCase().includes(keyword)
    );
  }

  return filtered;
});

const sendEmail = async () => {
  isLoading.value = true;
  try {
    await TopicService.sendEmailBatch({
      uuid: data.value.uuid,
      total: data.value.total,
    });
    await fetchData();
  } catch (e) {
    console.error(e);
  } finally {
    isLoading.value = false;
  }
};

const sendTestEmail = async () => {
  isLoading.value = true;
  try {
    var unTrimmedEmailList = receiverEmails.value.split(",");
    var trimmedEmailList = unTrimmedEmailList
      .map((email) => email.trim())
      .filter((email) => email !== "");
    await TopicService.sendtestEmailBatch({
      testEmails: trimmedEmailList,
      uuid: data.value.uuid,
    });
  } catch (e) {
    console.error(e);
  } finally {
    isLoading.value = false;
  }
};

const fetchData = async () => {
  isLoading.value = true;
  try {
    data.value = await TopicService.getEmailBatchList();
  } catch (e) {
    console.error(e);
  } finally {
    isLoading.value = false;
  }
};

const showDetails = async () => {
  isLoadingDetail.value = true;
  try {
    detailData.value = await TopicService.getEmailBatchDetail();
    // 重置搜索和筛选条件
    searchKeyword.value = "";
    failFilter.value = "all";
    dialogVisible.value = true;
  } catch (e) {
    console.error(e);
  } finally {
    isLoadingDetail.value = false;
  }
};

onMounted(() => {
  fetchData();
});
</script>

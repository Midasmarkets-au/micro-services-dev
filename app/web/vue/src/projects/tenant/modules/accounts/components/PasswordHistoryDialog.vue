<template>
  <el-dialog
    v-model="visible"
    :title="$t('action.viewPasswordHistory')"
    width="90%"
    :close-on-click-modal="false"
  >
    <div class="password-history">
      <!-- 筛选区域 -->
      <div class="filter-section mb-4">
        <el-select
          v-model="filters.passwordType"
          :placeholder="$t('fields.passwordType')"
          clearable
          style="width: 200px"
          @change="fetchHistory"
        >
          <el-option label="Main" value="main" />
          <el-option label="Investor" value="investor" />
          <el-option label="Phone" value="phone" />
        </el-select>
      </div>

      <!-- 表格 -->
      <el-table
        v-loading="loading"
        :data="historyData"
        stripe
        border
        style="width: 100%"
      >
        <el-table-column prop="id" label="ID" width="80" align="center" />
        <el-table-column
          prop="passwordType"
          :label="$t('fields.passwordType')"
          width="120"
          align="center"
        >
          <template #default="{ row }">
            <el-tag :type="getPasswordTypeTag(row.passwordType)" size="small">
              {{ row.passwordType }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column
          prop="operationType"
          :label="$t('fields.operationType')"
          width="140"
          align="center"
        >
          <template #default="{ row }">
            <el-tag
              :type="row.operationType === 'admin_change' ? 'warning' : 'info'"
              size="small"
            >
              {{ $t(`type.operationType.${row.operationType}`) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column
          prop="operatorName"
          :label="$t('fields.operator')"
          width="150"
        />
        <el-table-column
          prop="operatorRole"
          :label="$t('fields.operatorRole')"
          width="120"
          align="center"
        />
        <el-table-column
          prop="reason"
          :label="$t('fields.reason')"
          min-width="200"
        >
          <template #default="{ row }">
            <span v-if="row.reason" class="text-muted">{{ row.reason }}</span>
            <span v-else class="text-secondary">-</span>
          </template>
        </el-table-column>
        <el-table-column
          prop="success"
          :label="$t('fields.status')"
          width="100"
          align="center"
        >
          <template #default="{ row }">
            <el-tag :type="row.success ? 'success' : 'danger'" size="small">
              {{ row.success ? $t("status.success") : $t("status.failed") }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column
          prop="ipAddress"
          label="IP"
          width="150"
          align="center"
        />
        <el-table-column
          prop="changedOn"
          :label="$t('fields.changedOn')"
          width="180"
          align="center"
        >
          <template #default="{ row }">
            {{ formatDateTime(row.changedOn) }}
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <div class="pagination-section mt-4 d-flex justify-content-end">
        <el-pagination
          v-model:current-page="pagination.currentPage"
          v-model:page-size="pagination.pageSize"
          :page-sizes="[20, 50, 100]"
          :total="pagination.total"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="handleSizeChange"
          @current-change="handlePageChange"
        />
      </div>
    </div>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, reactive } from "vue";
import { ElNotification } from "element-plus";
import i18n from "@/core/plugins/i18n";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const t = i18n.global.t;

const visible = ref(false);
const loading = ref(false);
const accountId = ref(0);
const historyData = ref<any[]>([]);

const filters = reactive({
  passwordType: "",
});

const pagination = reactive({
  currentPage: 1,
  pageSize: 20,
  total: 0,
});

const getPasswordTypeTag = (type: string) => {
  const typeMap: Record<string, string> = {
    main: "primary",
    investor: "success",
    phone: "warning",
  };
  return typeMap[type] || "info";
};

const formatDateTime = (dateString: string) => {
  if (!dateString) return "-";
  const date = new Date(dateString);
  return date.toLocaleString();
};

const fetchHistory = async () => {
  if (!accountId.value) return;

  loading.value = true;
  try {
    const params: any = {
      limit: pagination.pageSize,
      offset: (pagination.currentPage - 1) * pagination.pageSize,
    };

    if (filters.passwordType) {
      params.passwordType = filters.passwordType;
    }

    const response = await AccountService.getPasswordHistory(
      accountId.value,
      params
    );

    historyData.value = response.items || [];
    pagination.total = response.total || 0;
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    loading.value = false;
  }
};

const handleSizeChange = () => {
  pagination.currentPage = 1;
  fetchHistory();
};

const handlePageChange = () => {
  fetchHistory();
};

const show = (_accountId: number) => {
  accountId.value = _accountId;
  visible.value = true;
  pagination.currentPage = 1;
  filters.passwordType = "";
  fetchHistory();
};

const hide = () => {
  visible.value = false;
  historyData.value = [];
  pagination.total = 0;
};

defineExpose({
  show,
  hide,
});
</script>

<style lang="scss" scoped>
.password-history {
  .filter-section {
    display: flex;
    gap: 12px;
    align-items: center;
  }

  .pagination-section {
    padding: 16px 0;
  }
}
</style>

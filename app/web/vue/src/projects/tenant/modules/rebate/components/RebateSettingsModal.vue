<template>
  <div class="modal-overlay" @click="closeModal">
    <div class="modal-content" @click.stop>
      <div class="modal-header">
        <h2>{{ $t("rebate.customerBaseRebateTable") }}</h2>
        <button class="btn-close" @click="closeModal">×</button>
      </div>

      <div class="modal-body">
        <!-- 加载状态 -->
        <div v-if="loading" class="loading-container">
          <p>{{ $t("action.loading") }}...</p>
        </div>

        <!-- 无数据提示 -->
        <div
          v-else-if="!loading && accountTypes.length === 0"
          class="no-data-container"
        >
          <p>{{ $t("rebate.noDataReceived") }}</p>
        </div>

        <!-- 配置内容 -->
        <div v-else class="settings-container">
          <!-- 账号类型选择 -->
          <div class="account-type-section">
            <h3>{{ $t("rebate.accountType") }}</h3>
            <div class="account-type-tabs">
              <button
                v-for="accountType in accountTypes"
                :key="accountType.accountTypeId"
                class="account-type-tab"
                :class="{
                  active: selectedAccountType === accountType.accountTypeId,
                }"
                @click="selectAccountType(accountType.accountTypeId)"
              >
                {{ $t(`fields.${accountType.accountTypeName.toLowerCase()}`) }}
              </button>
            </div>
          </div>

          <!-- 返佣设置 -->
          <div class="rebate-settings-section">
            <h3>{{ $t("rebate.rebateSettings") }}</h3>

            <!-- 标准设置 -->
            <div class="setting-group">
              <div class="setting-header">
                <h4>{{ $t("rebate.standardSettings") }}</h4>
                <div class="unified-control">
                  <input
                    v-model="unifiedStandardValue"
                    :placeholder="$t('rebate.unifiedValuePlaceholder')"
                    class="unified-input"
                  />
                  <button
                    class="btn btn-primary btn-sm"
                    @click="applyUnifiedStandard"
                  >
                    {{ $t("rebate.applyToAll") }}
                  </button>
                </div>
              </div>

              <div class="category-grid">
                <div
                  v-for="categoryId in Object.keys(
                    currentOption?.category || {}
                  )"
                  :key="categoryId"
                  class="category-item"
                >
                  <label class="category-label">
                    {{ categoryNameMap[categoryId] || categoryId }}
                  </label>
                  <input
                    v-model.number="currentOption.category[categoryId]"
                    type="number"
                    step="0.01"
                    class="category-input"
                  />
                </div>
              </div>
            </div>

            <!-- Pip 设置 -->
            <div class="setting-group">
              <div class="setting-header">
                <h4>
                  <span style="margin-right: 10px">{{
                    $t("rebate.pipSettings")
                  }}</span>
                  <el-button
                    class="ml-1"
                    @click="addPipOption()"
                    :icon="Plus"
                  ></el-button>
                </h4>
                <div class="unified-control">
                  <input
                    v-model="unifiedPipValue"
                    :placeholder="$t('rebate.unifiedValuePlaceholder')"
                    class="unified-input"
                  />
                  <el-tooltip
                    :content="$t('rebate.applyToExpandedOnly')"
                    placement="top"
                  >
                    <button
                      class="btn btn-primary btn-sm"
                      @click="applyUnifiedPip"
                    >
                      {{ $t("rebate.applyToAll") }}
                    </button>
                  </el-tooltip>
                </div>
              </div>

              <div class="pip-options">
                <div
                  v-for="key in Object.keys(
                    currentOption?.allowPipSetting || {}
                  )"
                  :key="key"
                  class="pip-option"
                >
                  <div class="option-header">
                    <div class="option-info">
                      <div class="option-field">
                        <label>name:</label>
                        <input
                          v-model="currentOption.allowPipSetting[key].name"
                          :disabled="key === '0' || key === '5'"
                          class="option-name-input"
                        />
                      </div>
                      <div class="option-field">
                        <label>{{ $t("fields.value") }}:</label>
                        <input
                          v-model.number="
                            currentOption.allowPipSetting[key].value
                          "
                          type="number"
                          :disabled="key === '0' || key === '5'"
                          class="option-value-input"
                          @change="updatePipOptionValue(key, $event)"
                        />
                      </div>
                    </div>
                    <div class="option-actions">
                      <button
                        class="btn-toggle"
                        :class="{ active: expandedPipOptions.includes(key) }"
                        @click="togglePipOption(key)"
                      >
                        {{
                          expandedPipOptions.includes(key)
                            ? $t("action.collapse")
                            : $t("action.expand")
                        }}
                      </button>
                      <el-button
                        v-if="key !== '0' && key !== '5'"
                        type="danger"
                        size="small"
                        :icon="Delete"
                        @click="
                          deletePipOption(
                            key,
                            currentOption.allowPipSetting[key].name
                          )
                        "
                      />
                    </div>
                  </div>

                  <div
                    v-if="expandedPipOptions.includes(key)"
                    class="option-content"
                  >
                    <div class="category-grid">
                      <div
                        v-for="categoryId in Object.keys(
                          currentOption.allowPipSetting[key].items || {}
                        )"
                        :key="categoryId"
                        class="category-item"
                      >
                        <label class="category-label">
                          {{ categoryNameMap[categoryId] || categoryId }}
                        </label>
                        <input
                          v-model.number="
                            currentOption.allowPipSetting[key].items[categoryId]
                          "
                          type="number"
                          step="0.00001"
                          class="category-input"
                        />
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Commission 设置 -->
            <div class="setting-group">
              <div class="setting-header">
                <h4>
                  <span style="margin-right: 10px">{{
                    $t("rebate.commissionSettings")
                  }}</span>
                  <el-button
                    class="ml-1"
                    @click="addCommissionOption()"
                    :icon="Plus"
                  ></el-button>
                </h4>
                <div class="unified-control">
                  <input
                    v-model="unifiedCommissionValue"
                    :placeholder="$t('rebate.unifiedValuePlaceholder')"
                    class="unified-input"
                  />
                  <el-tooltip
                    :content="$t('rebate.applyToExpandedOnly')"
                    placement="top"
                  >
                    <button
                      class="btn btn-primary btn-sm"
                      @click="applyUnifiedCommission"
                    >
                      {{ $t("rebate.applyToAll") }}
                    </button>
                  </el-tooltip>
                </div>
              </div>

              <div class="commission-options">
                <div
                  v-for="key in Object.keys(
                    currentOption?.allowCommissionSetting || {}
                  )"
                  :key="key"
                  class="commission-option"
                >
                  <div class="option-header">
                    <div class="option-info">
                      <div class="option-field">
                        <label>name:</label>
                        <input
                          v-model="
                            currentOption.allowCommissionSetting[key].name
                          "
                          :disabled="key === '0' || key === '5'"
                          class="option-name-input"
                        />
                      </div>
                      <div class="option-field">
                        <label>{{ $t("fields.value") }}:</label>
                        <input
                          v-model.number="
                            currentOption.allowCommissionSetting[key].value
                          "
                          type="number"
                          min="0"
                          :disabled="key === '0' || key === '5'"
                          class="option-value-input"
                          @change="updateCommissionOptionValue(key, $event)"
                        />
                      </div>
                    </div>
                    <div class="option-actions">
                      <button
                        class="btn-toggle"
                        :class="{
                          active: expandedCommissionOptions.includes(key),
                        }"
                        @click="toggleCommissionOption(key)"
                      >
                        {{
                          expandedCommissionOptions.includes(key)
                            ? $t("action.collapse")
                            : $t("action.expand")
                        }}
                      </button>
                      <el-button
                        v-if="key !== '0' && key !== '5'"
                        type="danger"
                        size="small"
                        :icon="Delete"
                        @click="
                          deleteCommissionOption(
                            key,
                            currentOption.allowCommissionSetting[key].name
                          )
                        "
                      />
                    </div>
                  </div>

                  <div
                    v-if="expandedCommissionOptions.includes(key)"
                    class="option-content"
                  >
                    <div class="category-grid">
                      <div
                        v-for="categoryId in Object.keys(
                          currentOption.allowCommissionSetting[key].items || {}
                        )"
                        :key="categoryId"
                        class="category-item"
                      >
                        <label class="category-label">
                          {{ categoryNameMap[categoryId] || categoryId }}
                        </label>
                        <input
                          v-model.number="
                            currentOption.allowCommissionSetting[key].items[
                              categoryId
                            ]
                          "
                          type="number"
                          step="0.01"
                          class="category-input"
                        />
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="modal-footer">
        <button class="btn btn-light" @click="closeModal">
          {{ $t("action.close") }}
        </button>
        <button
          class="btn btn-primary"
          @click="saveSettings"
          :disabled="loading"
        >
          {{ loading ? $t("action.saving") : $t("rebate.saveConfiguration") }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from "vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { useI18n } from "vue-i18n";
import rebateService from "../services/RebateService";
import { Plus, Delete } from "@element-plus/icons-vue";
const { t } = useI18n();

const emit = defineEmits<{
  close: [];
  save: [settings: any];
}>();

// 响应式数据
const selectedAccountType = ref<number | null>(null);
const unifiedStandardValue = ref("");
const unifiedPipValue = ref("");
const unifiedCommissionValue = ref("");
const expandedPipOptions = ref<string[]>([]);
const expandedCommissionOptions = ref<string[]>([]);
const loading = ref(false);

// API 返回的数据
const accountTypes = ref<any[]>([]);
const categoryNameMap = ref<Record<string, string>>({});
const apiData = ref<any>(null);

// 默认设置数据（用作fallback）
const defaultSettings = {
  "4": {
    standard: {
      OptionName: "Standard",
      Category: {
        "1": 8,
        "2": 12,
        "3": 20,
        "4": 120,
        "5": 24,
        "6": 20,
        "7": 50,
        "8": 5,
        "9": 30,
        "10": 60,
        "11": 70,
        "12": 5,
        "13": 20,
        "14": 10,
        "15": 2,
        "16": 5,
      },
      AllowPipOptions: [0, 5],
      AllowPipSetting: {
        "0": {
          Name: "No",
          Value: 0,
          Items: {
            "1": 0,
            "2": 0,
            "3": 0,
            "4": 0,
            "5": 0,
            "6": 0,
            "7": 0,
            "8": 0,
            "9": 0,
            "10": 0,
            "11": 0,
            "12": 0,
            "13": 0,
            "14": 0,
            "15": 0,
            "16": 0,
          },
        },
        "5": {
          Name: "P5",
          Value: 5,
          Items: {
            "1": 5e-5,
            "2": 0.005,
            "3": 0.05,
            "4": 0.005,
            "5": 0.005,
            "6": 0.005,
            "7": 0,
            "8": 0,
            "9": 0,
            "10": 0,
            "11": 0,
            "12": 0,
            "13": 0,
            "14": 0,
            "15": 0,
            "16": 0,
          },
        },
      },
      AllowCommissionOptions: [0, 5],
      AllowCommissionSetting: {
        "0": {
          Name: "No",
          Value: 0,
          Items: {
            "1": 0,
            "2": 0,
            "3": 0,
            "4": 0,
            "5": 0,
            "6": 0,
            "7": 0,
            "8": 0,
            "9": 0,
            "10": 0,
            "11": 0,
            "12": 0,
            "13": 0,
            "14": 0,
            "15": 0,
            "16": 0,
          },
        },
        "5": {
          Name: "5",
          Value: 5,
          Items: {
            "1": 5,
            "2": 5,
            "3": 5,
            "4": 5,
            "5": 5,
            "6": 5,
            "7": 0,
            "8": 0,
            "9": 0,
            "10": 0,
            "11": 0,
            "12": 0,
            "13": 0,
            "14": 0,
            "15": 0,
            "16": 0,
          },
        },
      },
    },
  },
  "6": {
    standard: {
      OptionName: "raw",
      Category: {
        "1": 2,
        "2": 2,
        "3": 2,
        "4": 2,
        "5": 2,
        "6": 2,
        "7": 2,
        "8": 2,
        "9": 2,
        "10": 2,
        "11": 2,
        "12": 2,
        "13": 2,
        "14": 2,
        "15": 2,
        "16": 2,
      },
      AllowPipOptions: [],
      AllowPipSetting: {},
      AllowCommissionOptions: [],
      AllowCommissionSetting: {},
    },
  },
};

// 计算属性 - 当前账号类型
const currentAccountType = computed(() => {
  return accountTypes.value.find(
    (type) => type.accountTypeId === selectedAccountType.value
  );
});

// 计算属性 - 当前选项（第一个options）
const currentOption = computed(() => {
  return currentAccountType.value?.options?.[0];
});

// 方法
const selectAccountType = (typeId: number) => {
  selectedAccountType.value = typeId;
};

const togglePipOption = (key: string) => {
  const index = expandedPipOptions.value.indexOf(key);
  if (index > -1) {
    expandedPipOptions.value.splice(index, 1);
  } else {
    expandedPipOptions.value.push(key);
  }
};

const toggleCommissionOption = (key: string) => {
  const index = expandedCommissionOptions.value.indexOf(key);
  if (index > -1) {
    expandedCommissionOptions.value.splice(index, 1);
  } else {
    expandedCommissionOptions.value.push(key);
  }
};

const applyUnifiedStandard = () => {
  const value = parseFloat(unifiedStandardValue.value);
  if (!isNaN(value) && currentOption.value) {
    Object.keys(currentOption.value.category).forEach((key) => {
      currentOption.value.category[key] = value;
    });
  }
};

const applyUnifiedPip = () => {
  const value = parseFloat(unifiedPipValue.value);
  if (!isNaN(value) && currentOption.value) {
    // 只对展开的选项进行设置
    if (expandedPipOptions.value.length === 0) {
      ElMessage.warning(t("rebate.noExpandedOptionsWarning"));
      return;
    }

    expandedPipOptions.value.forEach((pipKey) => {
      Object.keys(
        currentOption.value.allowPipSetting[pipKey]?.items || {}
      ).forEach((categoryKey) => {
        currentOption.value.allowPipSetting[pipKey].items[categoryKey] = value;
      });
    });

    ElMessage.success(
      t("rebate.appliedToExpandedOptions", {
        count: expandedPipOptions.value.length,
      })
    );
  }
};

const applyUnifiedCommission = () => {
  const value = parseFloat(unifiedCommissionValue.value);
  if (!isNaN(value) && currentOption.value) {
    // 只对展开的选项进行设置
    if (expandedCommissionOptions.value.length === 0) {
      ElMessage.warning(t("rebate.noExpandedOptionsWarning"));
      return;
    }

    expandedCommissionOptions.value.forEach((commissionKey) => {
      Object.keys(
        currentOption.value.allowCommissionSetting[commissionKey]?.items || {}
      ).forEach((categoryKey) => {
        currentOption.value.allowCommissionSetting[commissionKey].items[
          categoryKey
        ] = value;
      });
    });

    ElMessage.success(
      t("rebate.appliedToExpandedOptions", {
        count: expandedCommissionOptions.value.length,
      })
    );
  }
};

// 加载设置
const loadSettings = async () => {
  try {
    loading.value = true;
    const response = await rebateService.getDefaultRebateLevel();
    console.log("API Response:", response);

    // 处理不同的响应格式
    const data = response?.data || response;
    console.log("Processed data:", data);

    if (data && data.accountTypes) {
      // 保存整个API返回的数据
      apiData.value = data;
      accountTypes.value = data.accountTypes || [];
      categoryNameMap.value = data.categoryNameMap || {};

      console.log("accountTypes:", accountTypes.value);
      console.log("categoryNameMap:", categoryNameMap.value);

      // 默认选中第一个账号类型（或者Standard）
      if (accountTypes.value.length > 0) {
        const standardType = accountTypes.value.find(
          (type) => type.accountTypeName === "Standard"
        );
        selectedAccountType.value =
          standardType?.accountTypeId || accountTypes.value[0].accountTypeId;
        console.log("selectedAccountType:", selectedAccountType.value);
      }
    } else {
      console.warn("No data received from API");
      ElMessage.warning(t("rebate.noDataReceived"));
    }
  } catch (error) {
    console.error(t("rebate.loadSettingsFailed") + ":", error);
    ElMessage.error(t("rebate.loadSettingsFailed"));
  } finally {
    loading.value = false;
  }
};

const closeModal = () => {
  emit("close");
};

// 保存前重新同步 allowPipOptions 和 allowCommissionOptions
const syncOptionsBeforeSave = () => {
  accountTypes.value.forEach((accountType) => {
    // 遍历每个 accountType 的 options 数组
    if (accountType.options && Array.isArray(accountType.options)) {
      accountType.options.forEach((option: any) => {
        // 同步 allowPipOptions
        if (option.allowPipSetting) {
          const values = Object.values(option.allowPipSetting).map(
            (item: any) => item.value
          );
          // 去重并排序
          option.allowPipOptions = Array.from(new Set(values)).sort(
            (a: number, b: number) => a - b
          );
        }

        // 同步 allowCommissionOptions
        if (option.allowCommissionSetting) {
          const values = Object.values(option.allowCommissionSetting).map(
            (item: any) => item.value
          );
          // 去重并排序
          option.allowCommissionOptions = Array.from(new Set(values)).sort(
            (a: number, b: number) => a - b
          );
        }
      });
    }
  });
};

const saveSettings = async () => {
  try {
    loading.value = true;

    // 先关闭之前的所有提示消息
    ElMessage.closeAll();

    // 验证所有选项值是否都在国际化配置中
    const validation = validateOptionsBeforeSave();
    if (!validation.isValid) {
      const errorMessages: string[] = [];

      if (validation.invalidPipValues.length > 0) {
        const pipDetails = validation.invalidPipValues
          .map((item) => `${item.accountTypeName}: ${item.value}`)
          .join(", ");
        errorMessages.push(`Pip 值 [${pipDetails}] 不在预设范围`);
      }

      if (validation.invalidCommissionValues.length > 0) {
        const commissionDetails = validation.invalidCommissionValues
          .map((item) => `${item.accountTypeName}: ${item.value}`)
          .join(", ");
        errorMessages.push(`Commission 值 [${commissionDetails}] 不在预设范围`);
      }

      const fullMessage =
        errorMessages.join("；") +
        "。" +
        t("rebate.invalidValuesNeedI18nConfig");

      ElMessage.error({
        message: fullMessage,
        duration: 8000,
        showClose: true,
      });

      loading.value = false;
      return;
    }

    // 保存前重新同步数据，确保 allowPipOptions 和 allowCommissionOptions 与对应 Setting 中的 value 一致
    syncOptionsBeforeSave();

    // 按照API返回的结构原样提交
    const payload = {
      accountTypes: accountTypes.value,
      categoryNameMap: categoryNameMap.value,
    };
    await rebateService.updateDefaultRebateLevel(payload);
    ElMessage.success(t("rebate.saveSettingsSuccess"));
    emit("save", payload);
    emit("close");
  } catch (error) {
    console.error(t("rebate.saveSettingsFailed") + ":", error);
    ElMessage.error(t("rebate.saveSettingsFailed"));
  } finally {
    loading.value = false;
  }
};

// 检查值是否在国际化配置中
const isPipValueInI18n = (value: number): boolean => {
  const i18nKey = `type.pipOptions.${String(value)}`;
  const i18nValue = t(i18nKey);
  return i18nValue !== i18nKey;
};

const isCommissionValueInI18n = (value: number): boolean => {
  const i18nKey = `type.commissionOptions.${String(value)}`;
  const i18nValue = t(i18nKey);
  return i18nValue !== i18nKey;
};

// 获取 Pip 选项的国际化名称
const getPipOptionName = (value: number): string => {
  const i18nKey = `type.pipOptions.${String(value)}`;
  const i18nValue = t(i18nKey);

  // 如果国际化值等于 key，说明没有配置
  if (i18nValue === i18nKey) {
    console.warn(
      `当前 Pip 值 ${value} 不在预设范围，需联系开发配置语言翻译文件`
    );
    return `P${value}`;
  }

  return i18nValue;
};

// 获取 Commission 选项的国际化名称
const getCommissionOptionName = (value: number): string => {
  const i18nKey = `type.commissionOptions.${String(value)}`;
  const i18nValue = t(i18nKey);

  // 如果国际化值等于 key，说明没有配置
  if (i18nValue === i18nKey) {
    console.warn(
      `当前 Commission 值 ${value} 不在预设范围，需联系开发配置语言翻译文件`
    );
    return `C${value}`;
  }

  return i18nValue;
};

// 验证所有选项值是否都在国际化配置中
const validateOptionsBeforeSave = (): {
  isValid: boolean;
  invalidPipValues: Array<{ value: number; accountTypeName: string }>;
  invalidCommissionValues: Array<{ value: number; accountTypeName: string }>;
} => {
  const invalidPipValues: Array<{ value: number; accountTypeName: string }> =
    [];
  const invalidCommissionValues: Array<{
    value: number;
    accountTypeName: string;
  }> = [];

  accountTypes.value.forEach((accountType, accountTypeIndex) => {
    const accountTypeName = t(
      `fields.${accountType.accountTypeName?.toLowerCase() || "unknown"}`
    );

    if (accountType.options && Array.isArray(accountType.options)) {
      accountType.options.forEach((option: any, optionIndex) => {
        // 验证 Pip 选项
        if (option.allowPipSetting) {
          Object.values(option.allowPipSetting).forEach((item: any) => {
            if (!isPipValueInI18n(item.value)) {
              if (
                !invalidPipValues.find(
                  (v) =>
                    v.value === item.value &&
                    v.accountTypeName === accountTypeName
                )
              ) {
                invalidPipValues.push({ value: item.value, accountTypeName });
                console.warn(
                  `发现无效的 Pip 值: ${item.value}, 位置: accountType[${accountTypeIndex}] "${accountTypeName}".options[${optionIndex}], name: ${item.name}`
                );
              }
            }
          });
        }

        // 验证 Commission 选项
        if (option.allowCommissionSetting) {
          Object.values(option.allowCommissionSetting).forEach((item: any) => {
            if (!isCommissionValueInI18n(item.value)) {
              if (
                !invalidCommissionValues.find(
                  (v) =>
                    v.value === item.value &&
                    v.accountTypeName === accountTypeName
                )
              ) {
                invalidCommissionValues.push({
                  value: item.value,
                  accountTypeName,
                });
                console.warn(
                  `发现无效的 Commission 值: ${item.value}, 位置: accountType[${accountTypeIndex}] "${accountTypeName}".options[${optionIndex}], name: ${item.name}`
                );
              }
            }
          });
        }
      });
    }
  });

  return {
    isValid:
      invalidPipValues.length === 0 && invalidCommissionValues.length === 0,
    invalidPipValues,
    invalidCommissionValues,
  };
};

// 添加 Pip 选项
const addPipOption = () => {
  if (!currentOption.value) return;

  // 生成新的 key（找到最大的 key + 1）
  const keys = Object.keys(currentOption.value.allowPipSetting || {}).map(
    Number
  );
  const newKey = Math.max(...keys, 0) + 1;

  // 创建新的 items 对象（复制第一个项的结构，值设为 0）
  const firstKey = Object.keys(currentOption.value.allowPipSetting)[0];
  const templateItems = currentOption.value.category; // currentOption.value.allowPipSetting[firstKey].items;
  const newItems: Record<string, number> = {};

  Object.keys(templateItems).forEach((categoryId) => {
    newItems[categoryId] = 0;
  });

  // 创建新的 pip 选项，使用国际化名称
  const newOption = {
    name: getPipOptionName(newKey),
    value: newKey,
    items: newItems,
  };

  // 添加到 allowPipSetting
  currentOption.value.allowPipSetting[newKey] = newOption;

  // 更新 allowPipOptions 数组
  if (!currentOption.value.allowPipOptions) {
    currentOption.value.allowPipOptions = [];
  }
  currentOption.value.allowPipOptions.push(newKey);
  currentOption.value.allowPipOptions.sort((a: number, b: number) => a - b);
};

// 更新 Pip 选项的 value 值
const updatePipOptionValue = (oldKey: string, event: Event) => {
  if (!currentOption.value) return;

  const inputElement = event.target as HTMLInputElement;
  const newValue = Number(inputElement.value);
  const oldValue = Number(oldKey);

  // 如果值没有变化，直接返回
  if (newValue === oldValue) return;

  // 检查新值是否已经被其他 key 使用（数据结构冲突检查）
  if (currentOption.value.allowPipSetting[String(newValue)]) {
    // 如果新值对应的 key 已存在，恢复原值，阻止修改
    console.warn(
      `Pip value ${newValue} already exists, reverting to ${oldValue}`
    );
    // 恢复 input 显示值
    inputElement.value = String(oldValue);
    // 恢复数据模型中的值（v-model 已经改了，需要恢复）
    currentOption.value.allowPipSetting[oldKey].value = oldValue;
    return;
  }

  // 更新 allowPipOptions 数组
  if (currentOption.value.allowPipOptions) {
    const index = currentOption.value.allowPipOptions.indexOf(oldValue);
    if (index > -1) {
      currentOption.value.allowPipOptions[index] = newValue;
      currentOption.value.allowPipOptions.sort((a: number, b: number) => a - b);
    }
  }

  // 如果 key 改变了（value 作为 key），需要重新组织数据结构
  if (oldKey !== String(newValue)) {
    const optionData = currentOption.value.allowPipSetting[oldKey];
    delete currentOption.value.allowPipSetting[oldKey];

    // 更新 name 为对应的国际化名称
    optionData.name = getPipOptionName(newValue);

    currentOption.value.allowPipSetting[String(newValue)] = optionData;

    // 同步更新 expandedPipOptions 数组
    const expandedIndex = expandedPipOptions.value.indexOf(oldKey);
    if (expandedIndex > -1) {
      expandedPipOptions.value[expandedIndex] = String(newValue);
    }
  }
};

// 删除 Pip 选项
const deletePipOption = async (key: string, name: string) => {
  if (!currentOption.value) return;

  // 不能删除默认选项
  if (key === "0" || key === "5") {
    ElMessage.warning(t("rebate.cannotDeleteDefaultOption"));
    return;
  }
  await ElMessageBox.confirm(
    t("rebate.confirmDeletePip", { name: name }),
    t("tip.confirm"),
    {
      confirmButtonText: t("action.confirm"),
      cancelButtonText: t("action.cancel"),
      type: "warning",
    }
  );
  // 从 allowPipSetting 中删除
  delete currentOption.value.allowPipSetting[key];

  // 从 allowPipOptions 中删除
  const keyNum = Number(key);
  const index = currentOption.value.allowPipOptions.indexOf(keyNum);
  if (index > -1) {
    currentOption.value.allowPipOptions.splice(index, 1);
  }

  //ElMessage.success(t("rebate.pipOptionDeleted"));
};

// 添加 Commission 选项
const addCommissionOption = () => {
  if (!currentOption.value) return;

  // 生成新的 key（找到最大的 key + 1）
  const keys = Object.keys(
    currentOption.value.allowCommissionSetting || {}
  ).map(Number);
  const newKey = Math.max(...keys, 0) + 1;

  // 创建新的 items 对象（复制第一个项的结构，值设为 0）
  const firstKey = Object.keys(currentOption.value.allowCommissionSetting)[0];
  const templateItems = currentOption.value.category; //currentOption.value.allowCommissionSetting[firstKey].items;
  const newItems: Record<string, number> = {};

  Object.keys(templateItems).forEach((categoryId) => {
    console.log("categoryId", categoryId);
    newItems[categoryId] = 0;
  });

  // 创建新的 commission 选项，使用国际化名称
  const newOption = {
    name: getCommissionOptionName(newKey),
    value: newKey,
    items: newItems,
  };

  // 添加到 allowCommissionSetting
  currentOption.value.allowCommissionSetting[newKey] = newOption;

  // 更新 allowCommissionOptions 数组
  if (!currentOption.value.allowCommissionOptions) {
    currentOption.value.allowCommissionOptions = [];
  }
  currentOption.value.allowCommissionOptions.push(newKey);
  currentOption.value.allowCommissionOptions.sort(
    (a: number, b: number) => a - b
  );

  //ElMessage.success(t("rebate.commissionOptionAdded"));
};

// 更新 Commission 选项的 value 值
const updateCommissionOptionValue = (oldKey: string, event: Event) => {
  if (!currentOption.value) return;

  const inputElement = event.target as HTMLInputElement;
  const newValue = Number(inputElement.value);
  const oldValue = Number(oldKey);

  // 如果值没有变化，直接返回
  if (newValue === oldValue) return;

  // 检查新值是否已经被其他 key 使用（数据结构冲突检查）
  if (currentOption.value.allowCommissionSetting[String(newValue)]) {
    // 如果新值对应的 key 已存在，恢复原值，阻止修改
    console.warn(
      `Commission value ${newValue} already exists, reverting to ${oldValue}`
    );
    // 恢复 input 显示值
    inputElement.value = String(oldValue);
    // 恢复数据模型中的值（v-model 已经改了，需要恢复）
    currentOption.value.allowCommissionSetting[oldKey].value = oldValue;
    return;
  }

  // 更新 allowCommissionOptions 数组
  if (currentOption.value.allowCommissionOptions) {
    const index = currentOption.value.allowCommissionOptions.indexOf(oldValue);
    if (index > -1) {
      currentOption.value.allowCommissionOptions[index] = newValue;
      currentOption.value.allowCommissionOptions.sort(
        (a: number, b: number) => a - b
      );
    }
  }

  // 如果 key 改变了（value 作为 key），需要重新组织数据结构
  if (oldKey !== String(newValue)) {
    const optionData = currentOption.value.allowCommissionSetting[oldKey];
    delete currentOption.value.allowCommissionSetting[oldKey];

    // 更新 name 为对应的国际化名称
    optionData.name = getCommissionOptionName(newValue);

    currentOption.value.allowCommissionSetting[String(newValue)] = optionData;

    // 同步更新 expandedCommissionOptions 数组
    const expandedIndex = expandedCommissionOptions.value.indexOf(oldKey);
    if (expandedIndex > -1) {
      expandedCommissionOptions.value[expandedIndex] = String(newValue);
    }
  }
};

// 删除 Commission 选项
const deleteCommissionOption = async (key: string, name: string) => {
  if (!currentOption.value) return;

  // 不能删除默认选项
  if (key === "0" || key === "5") {
    ElMessage.warning(t("rebate.cannotDeleteDefaultOption"));
    return;
  }

  await ElMessageBox.confirm(
    t("rebate.confirmDeleteCommission", { name: name }),
    t("tip.confirm"),
    {
      confirmButtonText: t("action.confirm"),
      cancelButtonText: t("action.cancel"),
      type: "warning",
    }
  );

  // 从 allowCommissionSetting 中删除
  delete currentOption.value.allowCommissionSetting[key];

  // 从 allowCommissionOptions 中删除
  const keyNum = Number(key);
  const index = currentOption.value.allowCommissionOptions.indexOf(keyNum);
  if (index > -1) {
    currentOption.value.allowCommissionOptions.splice(index, 1);
  }

  //ElMessage.success(t("rebate.commissionOptionDeleted"));
};

// 加载数据
onMounted(() => {
  loadSettings();
});
</script>

<style scoped lang="scss">
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  border-radius: 8px;
  width: 90%;
  max-width: 1000px;
  max-height: 90vh;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  border-bottom: 1px solid #e5e7eb;

  h2 {
    margin: 0;
    font-size: 20px;
    font-weight: 600;
    color: #1f2937;
  }

  .btn-close {
    width: 32px;
    height: 32px;
    border: none;
    background: transparent;
    border-radius: 4px;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 20px;
    color: #6b7280;

    &:hover {
      background: #f3f4f6;
    }
  }
}

.modal-body {
  padding: 20px;
  flex: 1;
  overflow-y: auto;
}

.loading-container,
.no-data-container {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 300px;
  color: #6b7280;
  font-size: 14px;
}

.settings-container {
  .account-type-section {
    margin-bottom: 24px;

    h3 {
      font-size: 16px;
      font-weight: 600;
      color: #1f2937;
      margin: 0 0 12px 0;
    }

    .account-type-tabs {
      display: flex;
      gap: 8px;

      .account-type-tab {
        padding: 8px 16px;
        border: 1px solid #d1d5db;
        background: white;
        border-radius: 6px;
        cursor: pointer;
        font-size: 14px;
        color: #374151;
        transition: all 0.2s;

        &:hover {
          border-color: #3b82f6;
        }

        &.active {
          background: #3b82f6;
          color: white;
          border-color: #3b82f6;
        }
      }
    }
  }

  .rebate-settings-section {
    h3 {
      font-size: 16px;
      font-weight: 600;
      color: #1f2937;
      margin: 0 0 16px 0;
    }

    .setting-group {
      margin-bottom: 24px;
      border: 1px solid #e5e7eb;
      border-radius: 8px;
      overflow: hidden;

      .setting-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 16px 20px;
        background: #f9fafb;
        border-bottom: 1px solid #e5e7eb;

        h4 {
          margin: 0;
          font-size: 14px;
          font-weight: 600;
          color: #1f2937;
        }

        .unified-control {
          display: flex;
          gap: 8px;
          align-items: center;

          .unified-input {
            width: 120px;
            padding: 6px 8px;
            border: 1px solid #d1d5db;
            border-radius: 4px;
            font-size: 12px;
            color: #374151;

            &:focus {
              outline: none;
              border-color: #3b82f6;
              box-shadow: 0 0 0 2px rgba(59, 130, 246, 0.1);
            }
          }
        }
      }

      .category-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
        gap: 12px;
        padding: 16px 20px;

        .category-item {
          display: flex;
          flex-direction: column;
          gap: 4px;

          .category-label {
            font-size: 12px;
            font-weight: 500;
            color: #6b7280;
          }

          .category-input {
            padding: 6px 8px;
            border: 1px solid #d1d5db;
            border-radius: 4px;
            font-size: 12px;
            color: #374151;

            &:focus {
              outline: none;
              border-color: #3b82f6;
              box-shadow: 0 0 0 2px rgba(59, 130, 246, 0.1);
            }
          }
        }
      }

      .pip-options,
      .commission-options {
        .pip-option,
        .commission-option {
          border-bottom: 1px solid #e5e7eb;

          &:last-child {
            border-bottom: none;
          }

          .option-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 12px 20px;
            background: #f9fafb;

            .option-info {
              display: flex;
              gap: 16px;
              align-items: center;
              flex: 1;

              .option-field {
                display: flex;
                align-items: center;
                gap: 8px;

                label {
                  font-size: 13px;
                  font-weight: 500;
                  color: #6b7280;
                  min-width: 40px;
                }

                .option-name-input,
                .option-value-input {
                  padding: 6px 10px;
                  border: 1px solid #d1d5db;
                  border-radius: 4px;
                  font-size: 13px;
                  color: #1f2937;
                  transition: all 0.2s;

                  &:focus {
                    outline: none;
                    border-color: #3b82f6;
                    box-shadow: 0 0 0 2px rgba(59, 130, 246, 0.1);
                  }

                  &:disabled {
                    background: #f3f4f6;
                    color: #9ca3af;
                    cursor: not-allowed;
                  }
                }

                .option-name-input {
                  width: 120px;
                }

                .option-value-input {
                  width: 80px;
                }
              }
            }

            .option-actions {
              display: flex;
              gap: 8px;
              align-items: center;
            }

            h5 {
              margin: 0;
              font-size: 14px;
              font-weight: 500;
              color: #1f2937;
            }

            .btn-toggle {
              padding: 4px 8px;
              border: 1px solid #d1d5db;
              background: white;
              border-radius: 4px;
              cursor: pointer;
              font-size: 12px;
              color: #374151;
              transition: all 0.2s;

              &:hover {
                border-color: #3b82f6;
              }

              &.active {
                background: #3b82f6;
                color: white;
                border-color: #3b82f6;
              }
            }
          }

          .option-content {
            padding: 16px 20px;
            background: white;
          }
        }
      }
    }
  }
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 20px;
  border-top: 1px solid #e5e7eb;
}

.btn {
  padding: 8px 16px;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border: none;

  &.btn-primary {
    background: #3b82f6;
    color: white;

    &:hover {
      background: #2563eb;
    }
  }

  &.btn-secondary {
    background: #6b7280;
    color: white;

    &:hover {
      background: #4b5563;
    }
  }
}
</style>

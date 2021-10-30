/**
 * @file
 * Functions for system startup on FreeRTOS
 * @copyright 2019 Silicon Laboratories Inc.
 */
#ifndef _ZW_SYSTEM_STARTUP_H_
#define _ZW_SYSTEM_STARTUP_H_

#include <ZW_typedefs.h>
#include "ZW_application_transport_interface.h"

/**
 * Was the device woken up by an RTCC timer interrupt?
 *
 * This function can safely be called from multiple threads.
 *
 * @return true if woken up by RTCC timeout, false otherwise.
 */
bool IsWakeupCausedByRtccTimeout(void);

/**
 * Get number of milliseconds the device has spent sleeping
 * in EM4 before it was woken up.
 *
 * This function can safely be called from multiple threads.

 * @return Number of milliseconds.
 */
uint32_t GetCompletedSleepDurationMs(void);

/**
 * Get the sl_sleeptimer tick value of the device when it woke
 * up from EM4
 *
 * This function can safely be called from multiple threads.

 * @return RTCC ticks.
 */
uint32_t GetEM4WakeupTick(void);

/**
 * Get the sl_sleeptimer tick value of the device when it last
 * went to EM4 sleep. (It is stored in retention RAM)
 *
 * This function can safely be called from multiple threads.

 * @return RTCC ticks.
 */
uint32_t GetLastTickBeforeEM4(void);

/**
 * Please @see ZW_UserTask_ApplicationRegisterTask() in ZW_UserTask.h for more info.
 */
bool ZW_ApplicationRegisterTask(VOID_CALLBACKFUNC(appTaskFunc)(SApplicationHandles*),
                                uint8_t iZwRxQueueTaskNotificationBitNumber,
                                uint8_t iZwCommandStatusQueueTaskNotificationBitNumber,
                                const SProtocolConfig_t * pProtocolConfig);

/**
 * Used to get the pointer to the application handles structure!
 * Needed to insert task related data when creating user task.
 */
SApplicationHandles* ZW_system_startup_getAppHandles(void);

/**
 * Used by ZW_UserTask_ApplicationRegisterTask() in ZW_UserTask.h to set the
 * Event Notification Bit Numbers in the Application Interface structure!
 */
void ZW_system_startup_SetEventNotificationBitNumbers(uint8_t iZwRxQueueTaskNotificationBitNumber,
                                                      uint8_t iZwCommandStatusQueueTaskNotificationBitNumber,
                                                      const SProtocolConfig_t * pProtocolConfig);

/**
 * Used by ZW_UserTask_ApplicationRegisterTask() in ZW_UserTask.h to set the
 * Main Application Task Handle in the Application Interface structure,
 * after ZW_UserTask_ApplicationRegisterTask() has created the task!
 */
void ZW_system_startup_SetMainApplicationTaskHandle(TaskHandle_t xHandle);

/**
 * INCLUDE_xTaskGetSchedulerState did not include xTaskGetSchedulerState in the build.
 * This is a workaround!
 *
 * This function indicated whether the Scheduler has ever been started.
 * This will not indicate the current state, e.g. whether the scheduler
 * is in suspended state or running!
 */
bool ZW_system_startup_IsSchedulerStarted(void);

#endif /* _ZW_SYSTEM_STARTUP_H_ */
